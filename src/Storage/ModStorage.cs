using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Backend.Gamedesign;
using Backend.Managers;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

// Система хранения мода. Данные мода привязаны к состоянию игры и сохраняются в тот же файл сохранения
// отдельным свойством augustDaysMod. Игра неизвестные свойства при чтении пропускает, поэтому сохранения
// остаются совместимыми и с игрой без мода
namespace AugustDaysMod.Storage
{
    public class ModSaveData
    {
        [JsonProperty] public int version = 1;

        // Отношения союзных республик к центру, когда СССР не страна игрока
        [JsonProperty] public Dictionary<Countries, float> unionRelations = new Dictionary<Countries, float>();

        // Состояние Союза под управлением ИИ
        [JsonProperty] public UnionData union = new UnionData();

        // Флаги и счетчики событий мода
        [JsonProperty] public Dictionary<string, int> flags = new Dictionary<string, int>();

        // Новости для игрока, которые еще не показаны
        [JsonProperty] public List<string> news = new List<string>();

        [JsonIgnore]
        public bool IsEmpty => unionRelations.Count == 0 && flags.Count == 0 && news.Count == 0 && !union.active;

        public int Flag(string key) => flags.TryGetValue(key, out int value) ? value : 0;

        public void SetFlag(string key, int value = 1) => flags[key] = value;
    }

    public class UnionData
    {
        // Союз живет отдельно от игрока
        [JsonProperty] public bool active;

        // Сплоченность Союза, 0–100: насколько центр способен удерживать республики
        [JsonProperty] public float cohesion = 50f;

        // Союз распущен, республики независимы
        [JsonProperty] public bool dissolved;

        [JsonProperty] public DateTime? dissolvedDate;

        // Республики, объявившие независимость, и дата объявления
        [JsonProperty] public Dictionary<Countries, DateTime> independence = new Dictionary<Countries, DateTime>();
    }

    public static class ModStorage
    {
        public const string KEY = "augustDaysMod";

        private static readonly ConditionalWeakTable<GameState, ModSaveData> Table = new ConditionalWeakTable<GameState, ModSaveData>();

        // Копии состояния (например, для предпросмотра доктрин) своих данных не имеют и читают данные текущей игры
        public static ModSaveData For(GameState gameState)
        {
            if (Table.TryGetValue(gameState, out ModSaveData data))
            {
                return data;
            }

            GameState current = GameStateManager.inst != null ? GameStateManager.inst.GameState : null;
            if (current != null && !ReferenceEquals(current, gameState) && Table.TryGetValue(current, out data))
            {
                return data;
            }

            return Table.GetValue(gameState, _ => new ModSaveData());
        }

        public static void Attach(GameState gameState, ModSaveData data)
        {
            Table.Remove(gameState);
            Table.Add(gameState, data);
        }

        // Вызывается вместо JsonConvert.SerializeObject при записи сохранения
        public static string SerializeWithModData(object value, JsonSerializerSettings settings)
        {
            string json = JsonConvert.SerializeObject(value, settings);
            if (!(value is GameState gameState) || !Table.TryGetValue(gameState, out ModSaveData data) || data.IsEmpty)
            {
                return json;
            }

            int end = json.LastIndexOf('}');
            if (end < 0)
            {
                return json;
            }

            string modJson = JsonConvert.SerializeObject(data, settings);
            return json.Substring(0, end) + $",\"{KEY}\":{modJson}" + json.Substring(end);
        }

        // Вызывается вместо JsonConvert.DeserializeObject<GameState> при чтении сохранения
        public static GameState DeserializeWithModData(string json, JsonSerializerSettings settings)
        {
            GameState gameState = JsonConvert.DeserializeObject<GameState>(json, settings);
            if (gameState == null)
            {
                return null;
            }

            int index = json.LastIndexOf($"\"{KEY}\":", StringComparison.Ordinal);
            if (index < 0)
            {
                return gameState;
            }

            try
            {
                using (var reader = new JsonTextReader(new StringReader(json.Substring(index + KEY.Length + 3))))
                {
                    JToken token = JToken.ReadFrom(reader);
                    ModSaveData data = token.ToObject<ModSaveData>(JsonSerializer.Create(settings));
                    if (data != null)
                    {
                        Attach(gameState, data);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[AugustDays] данные мода в сохранении не читаются: {exception.Message}");
            }

            return gameState;
        }
    }

    // Запись и чтение сохранений идут через SaveLoadManager.Save и LoadSave. Подменяем в них сериализацию состояния
    [HarmonyPatch]
    public static class PatchSaveLoadManager
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(SaveLoadManager), nameof(SaveLoadManager.Save));
            yield return AccessTools.Method(typeof(SaveLoadManager), nameof(SaveLoadManager.LoadSave));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            MethodInfo serialize = AccessTools.Method(typeof(JsonConvert), nameof(JsonConvert.SerializeObject),
                new[] { typeof(object), typeof(JsonSerializerSettings) });
            MethodInfo deserialize = AccessTools.FirstMethod(typeof(JsonConvert), m =>
                    m.Name == nameof(JsonConvert.DeserializeObject) && m.IsGenericMethodDefinition && m.GetParameters().Length == 2 &&
                    m.GetParameters()[1].ParameterType == typeof(JsonSerializerSettings))
                .MakeGenericMethod(typeof(GameState));

            int replaced = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && Equals(instruction.operand, deserialize))
                {
                    instruction.operand = AccessTools.Method(typeof(ModStorage), nameof(ModStorage.DeserializeWithModData));
                    replaced++;
                }
                else if (instruction.opcode == OpCodes.Call && Equals(instruction.operand, serialize))
                {
                    instruction.operand = AccessTools.Method(typeof(ModStorage), nameof(ModStorage.SerializeWithModData));
                    replaced++;
                }

                yield return instruction;
            }

            if (replaced == 0)
            {
                Debug.LogWarning($"[AugustDays] ModStorage: в {original.Name} не найдена сериализация состояния");
            }
        }
    }
}
