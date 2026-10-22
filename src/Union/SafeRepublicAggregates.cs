using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Backend.Gamedesign.Utils;
using HarmonyLib;
using UnityEngine;

namespace AugustDaysMod.Union
{
    // Игра всегда считала, что у игрока есть союзные республики, и берет по ним среднее, максимум или первую без проверки.
    // У страны игрока без республик (например, независимой Украины) это роняет весь ежемесячный тик.
    // В методах, которые читают республики игрока, подменяем такие вызовы LINQ на версии, спокойно переносящие пустой список
    [HarmonyPatch]
    public static class SafeRepublicAggregates
    {
        // Классы игры, где республики игрока сводятся в одно значение
        private static readonly string[] TypeNames =
        {
            "Backend.Gamedesign.DoctrinesSystem.Doctrines.GameDoctrines.Doctrine15",
            "Backend.Gamedesign.EndingsSystem.Endings.GameEndings.Ending18",
            "Backend.Gamedesign.EventSystem.Events.GameEvents.Event226",
            "Backend.Gamedesign.EventSystem.Events.GameEvents.Event26",
            "Backend.Gamedesign.EventSystem.Events.GameEvents.Event9",
            "Backend.Gamedesign.Utils.ConditionsFormatter.ConditionsExtensions",
            "Backend.Gamedesign.Utils.CountryUtils",
            "Backend.Gamedesign.Utils.DoctrineUtils",
            "Backend.Gamedesign.Utils.PoliticUtils",
            "Backend.Gamedesign.Utils.UnitsOfWork.UnitOfWorkExtensions"
        };

        private static readonly HashSet<MethodInfo> RepublicGetters = new HashSet<MethodInfo>
        {
            AccessTools.Method(typeof(CountryUtils), nameof(CountryUtils.GetRepublicsOfPlayerKeys)),
            AccessTools.Method(typeof(CountryUtils), nameof(CountryUtils.GetRepublicsOfPlayer)),
            AccessTools.Method(typeof(CountryUtils), nameof(CountryUtils.GetRepublicsOfPlayerValues))
        };

        private static readonly Dictionary<string, MethodInfo> SafeBySignature = typeof(SafeLinq)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .ToDictionary(m => m.ToString());

        static IEnumerable<MethodBase> TargetMethods()
        {
            Assembly game = typeof(CountryUtils).Assembly;
            var targets = new List<MethodBase>();
            foreach (string typeName in TypeNames)
            {
                Type type = game.GetType(typeName);
                if (type == null)
                {
                    Debug.LogWarning($"[AugustDays] SafeRepublicAggregates: нет класса {typeName}");
                    continue;
                }

                foreach (Type candidate in new[] { type }.Concat(type.GetNestedTypes(AccessTools.all)))
                {
                    foreach (MethodInfo method in candidate.GetMethods(AccessTools.all | BindingFlags.DeclaredOnly))
                    {
                        if (NeedsPatch(method))
                        {
                            targets.Add(method);
                        }
                    }
                }
            }

            Debug.Log($"[AugustDays] SafeRepublicAggregates: методов с республиками игрока {targets.Count}");
            return targets;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) &&
                    instruction.operand is MethodInfo called && called.DeclaringType == typeof(Enumerable) &&
                    IsAggregate(called))
                {
                    MethodInfo safe = FindSafe(called);
                    if (safe != null)
                    {
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = safe;
                    }
                    else
                    {
                        Debug.LogWarning($"[AugustDays] SafeRepublicAggregates: нет безопасной замены для {called} в {original.DeclaringType}.{original.Name}");
                    }
                }

                yield return instruction;
            }
        }

        private static bool NeedsPatch(MethodInfo method)
        {
            if (method.IsAbstract || method.ContainsGenericParameters || method.GetMethodBody() == null)
            {
                return false;
            }

            List<CodeInstruction> body;
            try
            {
                body = PatchProcessor.GetOriginalInstructions(method);
            }
            catch (Exception)
            {
                return false;
            }

            bool readsRepublics = false;
            bool aggregates = false;
            foreach (CodeInstruction instruction in body)
            {
                if (instruction.operand is MethodInfo called)
                {
                    readsRepublics |= RepublicGetters.Contains(called);
                    aggregates |= called.DeclaringType == typeof(Enumerable) && IsAggregate(called);
                }
            }

            return readsRepublics && aggregates;
        }

        private static bool IsAggregate(MethodInfo method)
        {
            switch (method.Name)
            {
                case nameof(Enumerable.Average):
                case nameof(Enumerable.Max):
                case nameof(Enumerable.Min):
                case nameof(Enumerable.First):
                case nameof(Enumerable.Last):
                    return true;
                default:
                    return false;
            }
        }

        private static MethodInfo FindSafe(MethodInfo called)
        {
            MethodInfo definition = called.IsGenericMethod ? called.GetGenericMethodDefinition() : called;
            if (!SafeBySignature.TryGetValue(definition.ToString(), out MethodInfo safe))
            {
                return null;
            }

            return safe.IsGenericMethodDefinition ? safe.MakeGenericMethod(called.GetGenericArguments()) : safe;
        }
    }

    // Те же сигнатуры, что у System.Linq.Enumerable, но пустой список дает 0 или значение по умолчанию.
    // Имена параметров типов совпадают с Enumerable: по ним ищется замена
    public static class SafeLinq
    {
        public static float Average<TSource>(IEnumerable<TSource> source, Func<TSource, float> selector) =>
            source.Any() ? Enumerable.Average(source, selector) : 0f;

        public static double Average<TSource>(IEnumerable<TSource> source, Func<TSource, double> selector) =>
            source.Any() ? Enumerable.Average(source, selector) : 0d;

        public static double Average<TSource>(IEnumerable<TSource> source, Func<TSource, int> selector) =>
            source.Any() ? Enumerable.Average(source, selector) : 0d;

        public static float Average(IEnumerable<float> source) => source.Any() ? Enumerable.Average(source) : 0f;

        public static double Average(IEnumerable<double> source) => source.Any() ? Enumerable.Average(source) : 0d;

        public static double Average(IEnumerable<int> source) => source.Any() ? Enumerable.Average(source) : 0d;

        public static float Max<TSource>(IEnumerable<TSource> source, Func<TSource, float> selector) =>
            source.Any() ? Enumerable.Max(source, selector) : 0f;

        public static double Max<TSource>(IEnumerable<TSource> source, Func<TSource, double> selector) =>
            source.Any() ? Enumerable.Max(source, selector) : 0d;

        public static int Max<TSource>(IEnumerable<TSource> source, Func<TSource, int> selector) =>
            source.Any() ? Enumerable.Max(source, selector) : 0;

        public static float Max(IEnumerable<float> source) => source.Any() ? Enumerable.Max(source) : 0f;

        public static double Max(IEnumerable<double> source) => source.Any() ? Enumerable.Max(source) : 0d;

        public static int Max(IEnumerable<int> source) => source.Any() ? Enumerable.Max(source) : 0;

        public static float Min<TSource>(IEnumerable<TSource> source, Func<TSource, float> selector) =>
            source.Any() ? Enumerable.Min(source, selector) : 0f;

        public static double Min<TSource>(IEnumerable<TSource> source, Func<TSource, double> selector) =>
            source.Any() ? Enumerable.Min(source, selector) : 0d;

        public static int Min<TSource>(IEnumerable<TSource> source, Func<TSource, int> selector) =>
            source.Any() ? Enumerable.Min(source, selector) : 0;

        public static float Min(IEnumerable<float> source) => source.Any() ? Enumerable.Min(source) : 0f;

        public static double Min(IEnumerable<double> source) => source.Any() ? Enumerable.Min(source) : 0d;

        public static int Min(IEnumerable<int> source) => source.Any() ? Enumerable.Min(source) : 0;

        public static TSource First<TSource>(IEnumerable<TSource> source) => source.FirstOrDefault();

        public static TSource First<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            source.FirstOrDefault(predicate);

        public static TSource Last<TSource>(IEnumerable<TSource> source) => source.LastOrDefault();

        public static TSource Last<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
            source.LastOrDefault(predicate);
    }
}
