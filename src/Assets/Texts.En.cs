using System.Collections.Generic;
using System.Text;
using AugustDaysMod.Endings;
using AugustDaysMod.Events;

namespace AugustDaysMod.Assets
{
    // English texts of events and endings. Keys match the Russian dictionaries in Texts.cs
    public partial class Texts
    {
        private static Dictionary<int, Dictionary<int, string>> EventsTextEn = new Dictionary<int, Dictionary<int, string>>()
        {
            {
                Negotiations.ID, new Dictionary<int, string>()
                {
                    {Negotiations.TITLE, "Negotiations"},
                    {Negotiations.DESCRIPTION, new StringBuilder().
                        Append("At the meeting at the KGB's ABC facility on August 17 we agreed ").
                        Append("that a group consisting of Baklanov, Shenin, Boldin, Varennikov and Plekhanov would travel to Crimea to see Gorbachev ").
                        Append("and persuade him to declare a state of emergency. ").
                        ToString()
                    },
                    {Negotiations.ANSWER_1, "Try to persuade him to declare a state of emergency"},
                    {Negotiations.RESULT_1, new StringBuilder().
                        Append("In short, our meeting came to nothing. Its results were ").
                        Append("very vague, as was usually the case whenever ").
                        Append("Gorbachev had to make a decision on pressing ").
                        Append("issues or simply discuss a difficult subject. In ").
                        Append("the end he said: “To hell with you, do what you want. But ").
                        Append("report my opinion.” We exchanged glances: what opinion? Neither yes ").
                        Append("nor no? Do what you want - and what we had proposed was a ").
                        Append("state of emergency in certain regions of the country where ").
                        Append("people were dying, and in some sectors of the economy ").
                        Append("(the railways, for example). In other words, he gave his blessing to these ").
                        Append("measures, but did not want to declare the state of emergency himself. ").
                        ToString()
                    }
                }
            },
            {
                CoupBegin.ID, new Dictionary<int, string>()
                {
                    {CoupBegin.TITLE, "Preparing the Coup"},
                    {CoupBegin.DESCRIPTION, new StringBuilder().
                        Append("Another purpose of our trip to Foros to see Gorbachev was to derail the signing of the new Union Treaty scheduled for August 20, ").
                        Append("which in our view had no legal basis, since it had been drafted without the knowledge of the Cabinet of Ministers and the Supreme Soviet, ").
                        Append("and was meant to turn the USSR into an amorphous confederation of independent republics. We had already discussed this with him on August 4, but, as is well known, reached no agreement.").
                        AppendLine("<br>   I propose that we take preventive action and prepare surveillance of prominent opposition figures in the Soviet Union in order to prevent the signing of the new Union Treaty.").
                        ToString()
                    },
                    {CoupBegin.ANSWER_1, "Set up surveillance"},
                    {CoupBegin.ANSWER_2, "Prepare their arrests"},
                    {CoupBegin.ANSWER_3, "Counter the supporters of secession from the USSR"},
                    {CoupBegin.RESULT_1, new StringBuilder().
                        Append("Kryuchkov holds a meeting with the KGB leadership and orders ").
                        Append("teams from the central apparatus to be sent to Latvia, ").
                        Append("Estonia and Lithuania; he hands his deputy Valery Lebedev ").
                        Append("a list of 69 leaders of the democratic movement who ").
                        Append("are to be put under surveillance and, once the order is given, arrested. ").
                        Append("Rasshchepov, head of the 7th Directorate, is ordered ").
                        Append("to survey the situation around Boris Yeltsin's dacha, where, ").
                        Append("according to KGB information, he intends to go after ").
                        Append("returning from Alma-Ata. ").
                        ToString()
                    },
                    {CoupBegin.RESULT_2, new StringBuilder().
                        Append("Kryuchkov holds a meeting with the KGB leadership and orders ").
                        Append("teams from the central apparatus to be sent to Latvia, ").
                        Append("Estonia and Lithuania; he hands his deputy Valery Lebedev ").
                        Append("a list of 69 leaders of the democratic movement who ").
                        Append("are to be put under surveillance and, early on the morning of August 19, ").
                        Append("quietly arrested. Rasshchepov, head of the 7th Directorate, ").
                        Append("is also ordered to survey the situation around ").
                        Append("Boris Yeltsin's dacha, where, according to KGB ").
                        Append("information, he intends to go after returning from Alma-Ata.").
                        ToString()
                    },
                    {CoupBegin.RESULT_3, new StringBuilder().
                        Append("Kryuchkov holds a meeting with the KGB leadership and orders ").
                        Append("teams from the central apparatus to be sent to Latvia, ").
                        Append("Estonia and Lithuania; he hands his deputy Valery Lebedev ").
                        Append("a list of separatist movement leaders who are to be ").
                        Append("put under surveillance, compromised and then subjected ").
                        Append("to lustration, using every administrative means to ").
                        Append("break their resistance. Rasshchepov, head of the 7th Directorate, ").
                        Append("is ordered to survey the situation around ").
                        Append("Boris Yeltsin's dacha, where, according to KGB information, ").
                        Append("he intends to go after returning from Alma-Ata.").
                        ToString()
                    },
                }
            },
            {
                MiltaryAction.ID, new Dictionary<int, string>()
                {
                    {MiltaryAction.TITLE, "Wartime"},
                    {MiltaryAction.DESCRIPTION, new StringBuilder().
                        Append("Well, comrades. At exactly 01:00 you signed the draft Decree on ").
                        Append("assuming the office of President of the USSR and the Appeal ").
                        Append("of the USSR State Emergency Committee to ").
                        Append("the people. The Committee consists of you, Pavlov, Kryuchkov, Yazov, Pugo, ").
                        Append("Baklanov, Tizyakov and Starodubtsev. The GKChP has been convened, but should we ").
                        Append("bring in the troops, who are badly demoralized by the events in ").
                        Append("Tbilisi, Baku and Vilnius and are therefore in no hurry ").
                        Append("to take part in yet another adventure? ").
                        ToString()
                    },
                    {MiltaryAction.ANSWER_1, "Prepare loyal units, but wait"},
                    {MiltaryAction.ANSWER_2, "Send loyal units into major cities"},
                    {MiltaryAction.ANSWER_3, "Keep the troops in barracks without any information"},
                    {MiltaryAction.RESULT_1, new StringBuilder().
                        Append("At 07:00, on the orders of Defense Minister Yazov, the Taman ").
                        Append("Motor Rifle Division, the Kantemirovskaya Tank Division and the 106th (Tula) ").
                        Append("Airborne Division (about 4,000 men with 650 armored vehicles) ").
                        Append("move into Moscow. At 08:00 ").
                        Append("paratroopers take the Ostankino Television Center under guard. ").
                        Append("Transmitters are switched off, all channels are rerouted to ").
                        Append("Channel One, which broadcasts Lukyanov's statement and the GKChP ").
                        Append("documents, with classical music in between, mostly in ").
                        Append("minor keys. At 10:00 KGB and Interior Ministry special forces ").
                        Append("units block Manezhnaya Square and the Kremlin. ").
                        Append("The liberal appeal “To the Citizens of Russia!”, authored by ").
                        Append("a coalition of R. I. Khasbulatov, A. A. Sobchak, ").
                        Append("G. E. Burbulis, M. N. Poltoranin, S. M. Shakhrai and V. N. Yaroshenko, ").
                        Append("is circulated by fax among the opponents of our government. ").
                        ToString()
                    },
                    {MiltaryAction.RESULT_2, new StringBuilder().
                        Append("At 07:00, on the orders of Defense Minister Yazov, the Taman ").
                        Append("Motor Rifle Division, the Kantemirovskaya Tank Division and the 106th (Tula) ").
                        Append("Airborne Division (about 4,000 men with 650 armored vehicles) ").
                        Append("move into Moscow. ").
                        Append("At 08:00 paratroopers take the Ostankino Television Center under guard. ").
                        Append("Transmitters are switched off, all channels are rerouted to ").
                        Append("Channel One, which broadcasts Lukyanov's statement and the GKChP ").
                        Append("documents, with classical music in between, mostly in ").
                        Append("minor keys. From 10:00 onward, troops spend the day occupying the Central ").
                        Append("Telegraph, TASS, the Ostankino Television Center, radio stations, power plants, ").
                        Append("water pumping stations, bridges and their approaches. KGB and ").
                        Append("Interior Ministry special forces units block Manezhnaya ").
                        Append("Square and the Kremlin. ").
                        Append("<br>At 11:00 loyal units begin deploying ").
                        Append("in Kiev, Minsk, Tashkent, Riga, Kaunas, Vilnius and ").
                        Append("Kirovabad. The Supreme Soviets of the Ukrainian SSR and the Byelorussian SSR demand an explanation and ").
                        Append("the withdrawal of the troops. ").
                        Append("<br>The liberal appeal “To the Citizens of Russia!”, ").
                        Append("authored by a coalition of R. I. Khasbulatov, A. A. ").
                        Append("Sobchak, G. E. Burbulis, M. N. Poltoranin, S. M. Shakhrai and V. N. ").
                        Append("Yaroshenko, is intercepted by the KGB. ").
                        ToString()
                    },
                    {MiltaryAction.RESULT_3, new StringBuilder().
                        Append("At 07:00, on the orders of Defense Minister Yazov, communications ").
                        Append("are cut in all garrisons under the guise of secret ").
                        Append("military exercises; only the lines between unit commanders ").
                        Append("and their immediate superiors in Moscow remain open. Units of the Moscow Military District ").
                        Append("are strictly forbidden to leave their barracks. ").
                        Append("At 08:00 KGB and Interior Ministry ").
                        Append("special forces units take the ").
                        Append("Ostankino Television Center under guard. Transmitters are switched off, all ").
                        Append("channels are rerouted to Channel One, which broadcasts ").
                        Append("Lukyanov's statement and the GKChP documents, with ").
                        Append("classical music in between, mostly in minor keys. ").
                        Append("A ").
                        Append("liberal appeal “To the Citizens of Russia!” is drafted by ").
                        Append("a coalition of R. I. Khasbulatov, A. A. Sobchak, G. E. ").
                        Append("Burbulis, M. N. Poltoranin, S. M. Shakhrai and V. N. Yaroshenko, ").
                        Append("but the authors decide to hold off circulating it until ").
                        Append("you begin actively using armed force. ").
                        ToString()
                    }
                }
            },
            {
                YeltsinProblem.ID, new Dictionary<int, string>()
                {
                    {YeltsinProblem.TITLE, "The Yeltsin Problem"},
                    {YeltsinProblem.DESCRIPTION, new StringBuilder().
                        Append("Rasshchepov, head of the KGB 7th Directorate, reports that ").
                        Append("Boris Nikolayevich Yeltsin has been located at his dacha ").
                        Append("in the village of Arkhangelskoye. He already knows what is ").
                        Append("happening and is preparing his motorcade to leave urgently ").
                        Append("for Moscow in order to rally ").
                        Append("all his supporters in the upper echelons of RSFSR power ").
                        Append("to resist the GKChP. Alpha Group arrived on site at 04:00 ").
                        Append("and is ready to isolate this troublemaker.").
                        ToString()
                    },
                    {YeltsinProblem.ANSWER_1, "Isolate him and keep him in"},
                    {YeltsinProblem.ANSWER_2, "Eliminate the enemy of the people"},
                    {YeltsinProblem.ANSWER_3, "Only watch the dacha"},
                    {YeltsinProblem.RESULT_1, new StringBuilder().
                        Append("On August 19, 1991, 60 fighters of the USSR KGB special unit ").
                        Append("Alpha arrived at Yeltsin's dacha in Arkhangelskoye in two PAZ buses and two ").
                        Append("cars. ").
                        Append("They approached the dacha gates posing as paratroopers ").
                        Append("sent by the Defense Minister to reinforce ").
                        Append("security in the village. However, one of Yeltsin's guards ").
                        Append("recognized an Alpha officer who had taught at KGB courses. ").
                        Append("Yeltsin's guards invited the Alpha fighters to have lunch in ").
                        Append("the canteen. They declined, and within half an hour ").
                        Append("sealed off every possible exit and cut ").
                        Append("all communications. M. N. Poltoranin, who tried to get into the dacha, ").
                        Append("was arrested. The situation grew tense, but ").
                        Append("Yeltsin never dared to give the order to fight, ").
                        Append("nor to break out by force. The isolation was more than successful.").
                        ToString()
                    },
                    {
                        YeltsinProblem.RESULT_2, new StringBuilder().
                        Append("On August 19, 1991, 60 fighters of the USSR KGB special unit ").
                        Append("Alpha arrived at Yeltsin's dacha in Arkhangelskoye in two PAZ buses and two ").
                        Append("cars. ").
                        Append("They approached the dacha gates posing as paratroopers ").
                        Append("sent by the Defense Minister to reinforce ").
                        Append("security in the village. However, one of Yeltsin's guards ").
                        Append("recognized an Alpha officer who had taught at KGB courses. ").
                        Append("Yeltsin's guards invited the Alpha fighters to have lunch in ").
                        Append("the canteen. But the commandos immediately disarmed Korzhakov and ").
                        Append("his men, then blew up a gas pipe. ").
                        Append("Yeltsin, stepping out of his study, was instantly killed by a sniper.").
                        ToString()
                    },
                    {
                        YeltsinProblem.RESULT_3, new StringBuilder().
                        Append("On August 19, 1991, 60 fighters of the USSR KGB special unit ").
                        Append("Alpha arrived at Yeltsin's dacha in Arkhangelskoye in two PAZ buses and two ").
                        Append("cars. ").
                        Append("They approached the dacha gates posing as paratroopers ").
                        Append("sent by the Defense Minister to reinforce ").
                        Append("security in the village. However, one of Yeltsin's guards ").
                        Append("recognized an Alpha officer who had taught at KGB courses. ").
                        Append("Yeltsin's guards invited the Alpha fighters to have lunch in ").
                        Append("the canteen, which they gladly accepted, after which the ").
                        Append("signal came: “move to positions for Option No. 2.” At 08:00 ").
                        Append("a motorcade of two armored ZILs and two Volgas carrying ").
                        Append("guards was spotted, but the order to intercept it never ").
                        Append("came. Two hours later the order came: part of the unit ").
                        Append("was to guard Arkhangelskoye, the rest were to return to ").
                        Append("base, which was done. ").
                        ToString()
                    }
                }
            },
            {
                Broadcast.ID, new Dictionary<int, string>()
                {
                    {Broadcast.TITLE, "On the Air"},
                    {Broadcast.DESCRIPTION, new StringBuilder().
                        Append("From 11:00 people begin gathering on Manezhnaya Square with ").
                        Append("tricolor flags and portraits of Sakharov and Yeltsin. People's ").
                        Append("deputies of Russia and the Moscow City Council call on the crowd to ").
                        Append("go on an indefinite strike. The cry rings out: “Everyone to the defense ").
                        Append("of the White House!” The television center is in our hands. We must say something to the people!").
                        ToString()
                    },
                    {Broadcast.ANSWER_1, "Ask people to stay calm and follow instructions"},
                    {Broadcast.ANSWER_2, "Call for the formation of armed people's militias"},
                    {Broadcast.ANSWER_3, "Don't disturb the people, keep the classical music on"},
                    {Broadcast.RESULT_1, new StringBuilder().
                        Append("On the morning of August 19, city radio and television ").
                        Append("broadcast the Decree of the Vice President of the USSR, the Statement of the Soviet ").
                        Append("Leadership, the GKChP's Appeal to the Soviet People, the statement ").
                        Append("of the Chairman of the Supreme Soviet in its support, GKChP Resolutions No. 1 and ").
                        Append("No. 2, and the Decree of the Acting President of the USSR “On Introducing a State of ").
                        Append("Emergency in the City of Moscow”, followed by an address by ").
                        Append("Colonel General V. N. Samsonov, commander of the ").
                        Append("Leningrad Military District, whom the GKChP had appointed military ").
                        Append("commandant of Leningrad. The new Soviet leadership called on ").
                        Append("the people to stay calm, and on republic leaders and local ").
                        Append("authorities to follow current instructions and await ").
                        Append("new ones. The GKChP also sent an Appeal to the UN Secretary-General and ").
                        Append("foreign leaders, in which it ").
                        Append("reaffirmed all of the USSR's international obligations.").
                        ToString()
                    },
                    {Broadcast.RESULT_2, new StringBuilder().
                        Append("On the morning of August 19, city radio and television ").
                        Append("broadcast the Decree of the Vice President of the USSR, the Statement of the Soviet ").
                        Append("Leadership, the GKChP's Appeal to the Soviet People, the statement ").
                        Append("of the Chairman of the Supreme Soviet in its support, GKChP Resolutions No. 1 and ").
                        Append("No. 2, and the Decree of the Acting President of the USSR “On Introducing a State of ").
                        Append("Emergency in the City of Moscow”, followed by an address by ").
                        Append("Colonel General V. N. Samsonov, commander of the ").
                        Append("Leningrad Military District, whom the GKChP had appointed military ").
                        Append("commandant of Leningrad. He called on all ").
                        Append("communist movements in the country, Komsomol ").
                        Append("committees and party officials to convene and organize ").
                        Append("people's militias to keep order on the streets, with ").
                        Append("the direct assistance of the Union police. ").
                        Append("Nina Andreyeva was the first to respond, forming the very first ").
                        Append("Workers' Guard in Leningrad.").
                        ToString()
                    },
                    {Broadcast.RESULT_3, new StringBuilder().
                        Append("Despite the start of protests in Moscow, across the ").
                        Append("Soviet Union people are still utterly ").
                        Append("bewildered about what is actually going on in the country. Rumors ").
                        Append("are spreading, of course, but most people put ").
                        Append("it all down to yet another twist of Perestroika, of which ").
                        Append("there have been plenty over the past 7 years. And they await further ").
                        Append("developments. The first President of the USSR, meanwhile, is ill…").
                        ToString()
                    }
                }
            },
            {
                PoliticalDirection.ID, new Dictionary<int, string>()
                {
                    {PoliticalDirection.TITLE, "Our Position"},
                    {PoliticalDirection.DESCRIPTION, new StringBuilder().
                        Append("At 17:00 a press conference was held at the USSR Foreign Ministry press center ").
                        Append("by members of the GKChP - you, O. D. Baklanov, B. K. Pugo, V. A. ").
                        Append("Starodubtsev and A. I. Tizyakov. One of the GKChP's organizers, ").
                        Append("Prime Minister V. S. Pavlov, was absent: he had been ").
                        Append("hospitalized the day before with alcohol poisoning. ").
                        Append("The GKChP members were visibly nervous; footage of ").
                        Append("G. Yanayev's trembling hands went around the world (caused by a ").
                        Append("nervous system disorder aggravated by stress). 24-year-old ").
                        Append("journalist Tatyana Malkina openly called what was happening ").
                        Append("a “coup” and asked: “Tell me, please, ").
                        Append("do you understand that last night you carried out ").
                        Append("a coup d'état? And which comparison seems ").
                        Append("more apt to you - with 1917 or with 1964?” Now the whole ").
                        Append("world expects a clear political position from us. ").
                        ToString()
                    },
                    {PoliticalDirection.ANSWER_1, "Perestroika without excesses or separatism"},
                    {PoliticalDirection.ANSWER_2, "We will take the Chinese path"},
                    {PoliticalDirection.ANSWER_3, "Perestroika was a mistake, though it had some good initiatives"},
                    {PoliticalDirection.RESULT_1, new StringBuilder().
                        Append("In a brief statement {0} stressed that he had assumed ").
                        Append("the duties of President of the USSR under Article ").
                        Append("127-7 of the USSR Constitution. He said that the USSR GKChP was fully ").
                        Append("aware of the depth of the crisis gripping the country and ").
                        Append("was taking responsibility for the fate of the Motherland. Answering ").
                        Append("a question about where M. S. Gorbachev was now, the Acting President said ").
                        Append("that he was resting and receiving treatment in Crimea. “Over these years he ").
                        Append("has grown very tired and needs some time to ").
                        Append("recover his health.” {1} called Gorbachev his ").
                        Append("“friend” and expressed hope that after his rest he would return ").
                        Append("to duty and they would work together. “We will follow the course ").
                        Append("he began in 1985,” the Acting President added. ").
                        Append("Speaking of M. S. Gorbachev's role in the life of our society in ").
                        Append("recent years, {2} noted: “I believe ").
                        Append("Mikhail Sergeyevich did immeasurably much to ").
                        Append("launch democratic processes across the country in 1985. ").
                        Append("This man deserves every respect; he did ").
                        Append("everything to set us on the democratic path.” ").
                        ToString()
                    },
                    {PoliticalDirection.RESULT_2, new StringBuilder().
                        Append("Unexpectedly, it was not Acting President {0} who took the floor, but Minister ").
                        Append("of Internal Affairs {1}, who declared his commitment to the ").
                        Append("Chinese course - an uncompromising fight against separatists, ").
                        Append("subversive elements and wreckers - firmly confirming ").
                        Append("the intention to deepen further economic reforms and ").
                        Append("preserve those democratic changes which ").
                        Append("had undoubtedly made the Soviet state machine better, while ").
                        Append("scrapping misguided experiments. “One should not assume ").
                        Append("that some fundamentally new measures, never used before, ").
                        Append("will be found and proposed,” the head of the USSR ").
                        Append("Interior Ministry replied. “We must rely on professionals.” ").
                        ToString()
                    },
                    {PoliticalDirection.RESULT_3, new StringBuilder().
                        Append("The journalist's question, as never before, prompted ").
                        Append("the GKChP leaders with the right answer: “Yes, this is a new 1964!” Declaring ").
                        Append("their loyalty to the original aims of Perestroika - a course toward collective ").
                        Append("leadership, improvement of the Soviet state ").
                        Append("machine and the fight against dogmatism - the Acting President announced that ").
                        Append("Gorbachev himself had departed from these three principles, that his ").
                        Append("activities had been marked by subjectivism, high-handedness and ").
                        Append("voluntarism, and that numerous gross ").
                        Append("political and economic mistakes had been made. And the GKChP had convened to ").
                        Append("correct them. ").
                        ToString()
                    },
                }
            },
            {
                SovietsPalace.ID, new Dictionary<int, string>()
                {
                    {SovietsPalace.TITLE, "House of Soviets"},
                    {SovietsPalace.DESCRIPTION, new StringBuilder().
                        Append("At 12:00 a rally began near the White House, ").
                        Append("sanctioned by the Moscow city authorities. By some estimates, ").
                        Append("more than 200,000 people gathered. The rally was organized by ").
                        Append("democratic associations and the Councils of Labor ").
                        Append("Collectives of Moscow and the Moscow Region. The officially ").
                        Append("declared slogan of the rally was “For Law and Order.” ").
                        Append("The entire area around the White House is filled with tens of thousands of ").
                        Append("people. The rally lasted five hours. From the balcony of the White House ").
                        Append("I. Silaev, R. Khasbulatov, A. Rutskoy, G. Popov, A. ").
                        Append("Yakovlev, E. Shevardnadze and many others spoke. The House of Soviets has become ").
                        Append("the central headquarters of our entire opposition, around which ").
                        Append("up to 50,000 supporters of the democrats have gathered, while inside the ").
                        Append("building, according to the KGB, there are up to 500 armed militants. ").
                        ToString()
                    },
                    {SovietsPalace.YELTSIN_ALIVE, new StringBuilder().
                        Append("<br>In its evening edition of the program “Vremya”, USSR Central Television ").
                        Append("unexpectedly airs a report prepared ").
                        Append("by its correspondent Sergei Medvedev on the situation at ").
                        Append("the White House, which shows Yeltsin reading out ").
                        Append("the Decree “On the Illegality of the GKChP's Actions” signed the day before. ").
                        Append("It ends with Medvedev's commentary, in which he openly ").
                        Append("voices doubt that this report could ever ").
                        Append("make it on air. Nevertheless, the report was seen by a huge audience ").
                        Append("of viewers across the country, and it stood in stark contrast to ").
                        Append("the rest of the program. This was done with the direct ").
                        Append("approval of VGTRK director Leonid Kravchenko, a supporter of ").
                        Append("absolute Glasnost, who believed it necessary to give the floor ").
                        Append("not only to the GKChP, but also to its opponents. ").
                        ToString()
                    },
                    {SovietsPalace.ANSWER_1, "Cordon off the building with troops and Interior Ministry forces"},
                    {SovietsPalace.ANSWER_2, "Order the storming of the building"},
                    {SovietsPalace.ANSWER_3, "Draw up a plan, but don't storm"},
                    {SovietsPalace.RESULT_1, new StringBuilder().
                        Append("The GKChP drew up a plan to cordon off the White House and ").
                        Append("the protesters around it, so as to keep new people from getting in and ").
                        Append("information from getting out, letting through only citizens and ").
                        Append("deputies leaving the rallies for home. Units totaling about ").
                        Append("15,000 men were assigned to the ").
                        Append("operation. As a result, the rally gradually ").
                        Append("died down and order was preserved for the time being. However, ").
                        Append("the remaining protesters pitched tents, and who can guarantee ").
                        Append("that a new rally won't form in another part of the city?… ").
                        ToString()
                    },
                    {SovietsPalace.RESULT_2_OK, new StringBuilder().
                        Append("Tanks opened the operation. As planned, they fired ").
                        Append("blank rounds at close range and cleared passages through ").
                        Append("the barricades. Then fighters of the Dzerzhinsky Separate ").
                        Append("Motor Rifle Division wedged into the ranks of the defenders, pushed them apart, ").
                        Append("cleared the way to the White House entrances and held ").
                        Append("the “corridors.” Tula paratroopers entered the “corridors” and, ").
                        Append("using vehicles, broke through the doors and glazed openings in ").
                        Append("the walls, then fought their way through the building's floors. At that ").
                        Append("moment Alpha fighters, acting under a separate plan, ").
                        Append("searched the White House and neutralized ").
                        Append("the leaders of the resistance. Units totaling about 15,000 men ").
                        Append("were assigned to the operation, ").
                        Append("and it ended in success. Despite the deaths of five ").
                        Append("civilians, the opposition forces inside the White House were crushed, ").
                        Append("the protesters were split into isolated pockets, and the rally ").
                        Append("gradually died down. ").
                        ToString()
                    },
                    {SovietsPalace.RESULT_2_FAIL, new StringBuilder().
                        Append("Between 13:00 and 16:00, according to the BBC, generals of the Ministry ").
                        Append("of Defense, the KGB and the Interior Ministry - V. A. Achalov, V. F. Grushko, G. E. ").
                        Append("Ageyev, B. V. Gromov, A. I. Lebed, V. F. Karpukhin, V. ").
                        Append("I. Varennikov and B. P. Beskov - on the orders of the GKChP, ").
                        Append("prepared a previously unplanned seizure of the building ").
                        Append("of the RSFSR Supreme Soviet by security forces. According to ").
                        Append("experts, the seizure plan they drew up was ").
                        Append("flawless from a military point of view. However, General Lebed, ").
                        Append("one of the authors of the assault plan, and 10 tanks of the Taman ").
                        Append("Division (without ammunition) went over to the opposition, and ").
                        Append("then the commanders of Alpha and Vympel asked Deputy ").
                        Append("KGB Chairman Ageyev to call off the operation, after which ").
                        Append("Yazov did not dare to carry it out with the army alone and personally ").
                        Append("canceled the operation. ").
                        ToString()
                    },
                    {SovietsPalace.RESULT_3, new StringBuilder().
                        Append("Between 13:00 and 16:00, according to the BBC, generals of the Ministry ").
                        Append("of Defense, the KGB and the Interior Ministry - V. A. Achalov, V. F. Grushko, G. E. ").
                        Append("Ageyev, B. V. Gromov, A. I. Lebed, V. F. Karpukhin, V. ").
                        Append("I. Varennikov and B. P. Beskov - on the orders of the GKChP, ").
                        Append("prepared a previously unplanned seizure of the building ").
                        Append("of the RSFSR Supreme Soviet by security forces. According to ").
                        Append("experts, the seizure plan they drew up was ").
                        Append("flawless from a military point of view. However, no decision was ").
                        Append("made, since it could have led to casualties among the people ").
                        Append("at the House of Soviets, which would only have deepened ").
                        Append("the army's demoralization. Subsequent events only reinforced this view ").
                        Append("among the GKChP members: General Lebed and 10 tanks of the Taman ").
                        Append("Division (without ammunition) went over to the opposition, and Yazov ").
                        Append("decided not to inflame the situation among his subordinates. ").
                        ToString()
                    }
                }
            },
            {
                Foros.ID, new Dictionary<int, string>()
                {
                    {Foros.TITLE, "Foros"},
                    {Foros.DESCRIPTION, new StringBuilder().
                        Append("Comrade! Our power has stabilized, but Gorbachev is still ").
                        Append("in Foros with acute rheumatism, and we need to ").
                        Append("decide what to do with him after all, and what to tell the people afterwards… ").
                        ToString()
                    },
                    {Foros.ANSWER_1, "Force Gorbachev to recognize us as legitimate"},
                    {Foros.ANSWER_2, "Continue the “war of attrition”"},
                    {Foros.RESULT_1_OK, new StringBuilder().
                        Append("Seeing that we had firmly taken power, Gorbachev, under the pressure ").
                        Append("of brute force and facts, agreed to sign documents on his ").
                        Append("resignation from all posts and the transfer of full power to us, ").
                        Append("in exchange for guarantees that he would be allowed to leave the country ").
                        Append("and go abroad within a month. ").
                        ToString()
                    },
                    {Foros.RESULT_1_FAIL, new StringBuilder().
                        Append("Gorbachev told the arriving GKChP members that he could not fly to ").
                        Append("Moscow because he was “in a corset and his leg had given out.” ").
                        Append("The president also said that he “would fly to Moscow in any case ").
                        Append("for the signing of the treaty, even if they cut off his leg,” ").
                        Append("and refused to sign any documents. We left empty-handed. ").
                        Append("This sows confusion among the GKChP leadership; Yazov and Kryuchkov ").
                        Append("speak doubtfully of the GKChP's future (the defense ").
                        Append("minister said literally the following: “We've lost. We knew how to ").
                        Append("make mischief, now we must know how to answer for it. I'll fly to Mikhail Sergeyevich to repent”).").
                        ToString()
                    },
                    {Foros.RESULT_2, "Gorbachev remains in Foros, and a degree of order persists in the country. Everything goes on as before…"}
                }
            },
            {
                Final.ID, new Dictionary<int, string>()
                {
                    {Final.TITLE, "Outcome"},
                    {Final.DESCRIPTION_OK, new StringBuilder().
                        Append("16:00: an orderly withdrawal of troops began from Moscow, where ").
                        Append("order had been restored. On August 23 the GKChP was recognized by ").
                        Append("the leadership of all the Union republics, including the Baltic states, ").
                        Append("Georgia, Moldavia and Armenia, as well as by embassies and groups of forces ").
                        Append("outside the USSR. On August 25 an extraordinary session of the USSR Supreme ").
                        Append("Soviet finally legitimized the GKChP and ").
                        Append("decided to extend the state of emergency for 6 years. ").
                        Append("Gennady Ivanovich Yanayev was sworn in as ").
                        Append("Acting President of the USSR. On August 26 a Plenum of the CPSU ").
                        Append("Central Committee elected Gennady Yanayev General Secretary of the CPSU Central Committee, ").
                        Append("after which the GKChP was recognized by the party committees. ").
                        Append("On August 27 the new Soviet leadership was ").
                        Append("recognized by the USA, China, France and West Germany. ").
                        ToString()
                    },
                    {Final.DESCRIPTION_FAIL, new StringBuilder().
                        Append("16:00: the Presidium of the USSR Supreme Soviet adopted ").
                        Append("a resolution declaring illegal the de facto ").
                        Append("removal of the President of the USSR from his duties and ").
                        Append("their transfer to the country's Vice President, and accordingly ").
                        Append("demanded the repeal of the decrees and the resolutions ").
                        Append("on the state of emergency based on them, as legally ").
                        Append("void from the moment of signing. ").
                        Append("<br>At 16:52 RSFSR Vice President A. V. Rutskoy, RSFSR Prime Minister I. ").
                        Append("S. Silaev, Primakov, Bakatin, Dunayev, Russian Minister of Justice ").
                        Append("Fyodorov and 36 police officers armed with assault rifles fly ").
                        Append("to Foros to Gorbachev, landing at Belbek at 19:16.").
                        Append("<br>17:00: Vice President {0} signed a decree declaring the GKChP ").
                        Append("dissolved and all its decisions void. ").
                        Append("<br>22:00: RSFSR Prosecutor General Valentin ").
                        Append("Stepankov issues a warrant for the arrest of the former GKChP members. ").
                        ToString()
                    },
                    {Final.ANSWER_1, "What happened next?"},
                    {Final.RESULT_1, new StringBuilder().
                        Append("Once our power had stabilized, Kryuchkov and the GKChP decided ").
                        Append("to release Gorbachev, on condition that he announce his ").
                        Append("resignation as President of the USSR for health reasons and ").
                        Append("leave for treatment in West Germany. And so it happened: he settled at ").
                        Append("his German dacha (while receiving a pension and enjoying ").
                        Append("other privileges as a former president). Recently, however, he gave ").
                        Append("an interview to Western media in which he said that he had never ").
                        Append("actually resigned and that our rule is illegitimate… ").
                        ToString()
                    },
                    {Final.YELTSIN_ALIVE, new StringBuilder().
                        Append("<br>Remaining President of the RSFSR, Yeltsin and his team will ").
                        Append("continue to resist our policies. ").
                        Append("Boris Nikolayevich has already declared that he “will fight the junta to the bitter end.” ").
                        ToString()
                    },
                    {Final.YELTSIN_INACTIVE, new StringBuilder().
                        Append("<br>It was decided to release Yeltsin, having first ").
                        Append("removed him from the post of President of the RSFSR for unconstitutional ").
                        Append("actions and expelled him from the Russian government. Russian ").
                        Append("institutions have been purged of his supporters. A new, more moderate ").
                        Append("RSFSR government has been formed, headed by ").
                        Append("Nikolai Ryzhkov, but Yeltsin is beginning to gather ").
                        Append("a radical opposition around himself.").
                        ToString()
                    }
                }
            },
            {
                ForosDelegation.ID, new Dictionary<int, string>()
                {
                    {ForosDelegation.TITLE, "Delegation in Foros"},
                    {ForosDelegation.DESCRIPTION, new StringBuilder().
                        Append("August 18, 16:50. Medvedev, head of your security detail, reports: arriving at the dacha unannounced are ").
                        Append("Baklanov, Shenin, Boldin, Varennikov and Plekhanov, head of the KGB 9th Directorate. The telephones, ").
                        Append("including the government line, are already dead. The visitors announce that a committee ").
                        Append("on the state of emergency has been formed and demand that you sign a decree imposing it nationwide — ").
                        Append("or hand your powers over to Vice President Yanayev “while you are ill.”").
                        ToString()
                    },
                    {ForosDelegation.ANSWER_1, "Refuse: this is an adventure"},
                    {ForosDelegation.ANSWER_2, "Sign the state of emergency decree"},
                    {ForosDelegation.ANSWER_3, "Play for time: let the Supreme Soviet decide"},
                    {ForosDelegation.RESULT_1, new StringBuilder().
                        Append("“You and those who sent you are adventurers. You will destroy both yourselves and the country.” Varennikov demands ").
                        Append("your resignation; you refuse to sign anything. The delegation leaves empty-handed, ").
                        Append("border guards block the road out of the dacha, and patrol boats take up station in the bay.").
                        ToString()
                    },
                    {ForosDelegation.RESULT_2, new StringBuilder().
                        Append("The decree is signed. On the morning of August 19 the country learns of the state of emergency from the president, ").
                        Append("not from Yanayev. The GKChP becomes a body of power under the president, and troops enter Moscow. ").
                        Append("The West is stunned; Yeltsin calls it an unconstitutional coup.").
                        ToString()
                    },
                    {ForosDelegation.RESULT_3, new StringBuilder().
                        Append("You say that only the Supreme Soviet can take such measures, and dodge a direct answer. ").
                        Append("The delegation takes back to Moscow what it wanted to hear: “To hell with you, do what you want.” ").
                        Append("The connection to the dacha is never restored.").
                        ToString()
                    }
                }
            },
            {
                ForosIsolation.ID, new Dictionary<int, string>()
                {
                    {ForosIsolation.TITLE, "Isolation in Foros"},
                    {ForosIsolation.DESCRIPTION, new StringBuilder().
                        Append("On the morning of August 19 the guards' transistor radio reports: owing to the president's “inability for health ").
                        Append("reasons” to perform his duties, his powers pass to Yanayev, and power is assumed by ").
                        Append("the State Emergency Committee (GKChP). Tanks are entering Moscow. ").
                        Append("The dacha is cut off from the world, but the 32-strong security detail remains loyal to the president.").
                        ToString()
                    },
                    {ForosIsolation.ANSWER_1, "Record a video address to the people"},
                    {ForosIsolation.ANSWER_2, "Try to break through to Yalta with the guards"},
                    {ForosIsolation.ANSWER_3, "Wait for news from Moscow"},
                    {ForosIsolation.RESULT_1, new StringBuilder().
                        Append("At night your son-in-law Anatoly films your statement on a home video camera: the president is healthy, ").
                        Append("the GKChP's actions are illegal. The tape is cut into pieces and hidden, to be smuggled out at the first opportunity.").
                        ToString()
                    },
                    {ForosIsolation.RESULT_2, new StringBuilder().
                        Append("The security cars run up against border guard trucks. It does not come to shooting, ").
                        Append("but word that the president is being held by force spreads among KGB troops in Crimea.").
                        ToString()
                    },
                    {ForosIsolation.RESULT_3, new StringBuilder().
                        Append("The days drag on in uncertainty. The BBC and Radio Liberty report barricades going up at the White House, ").
                        Append("while Moscow wonders whether the president is alive and whether he has signed anything.").
                        ToString()
                    }
                }
            },
            {
                WhiteHouseDefense.ID, new Dictionary<int, string>()
                {
                    {WhiteHouseDefense.TITLE, "The White House"},
                    {WhiteHouseDefense.DESCRIPTION, new StringBuilder().
                        Append("On August 20 tens of thousands gather at the RSFSR House of Soviets. The GKChP is preparing an assault: ").
                        Append("Operation Thunder is to be carried out by Alpha Group, paratroopers and Interior Ministry troops. ").
                        Append("From Foros you can do nothing — the outcome will be decided by those who give and carry out orders in Moscow.").
                        ToString()
                    },
                    {WhiteHouseDefense.ANSWER_1, "Listen to the news from Moscow"},
                    {WhiteHouseDefense.RESULT_1, new StringBuilder().
                        Append("On the night of August 21, three defenders die on the Garden Ring. ").
                        Append("By morning it becomes clear who won that night.").
                        ToString()
                    }
                }
            },
            {
                ForosPlane.ID, new Dictionary<int, string>()
                {
                    {ForosPlane.TITLE, "Planes to Foros"},
                    {ForosPlane.DESCRIPTION, new StringBuilder().
                        Append("On August 21 troops are withdrawn from Moscow: the GKChP has won the support of neither the army nor the republics. ").
                        Append("Kryuchkov, Yazov, Baklanov and Tizyakov fly to Belbek on one plane — to beg forgiveness. ").
                        Append("Right behind them takes off a plane carrying RSFSR Vice President Rutskoy, Prime Minister Silaev, Bakatin and Primakov.").
                        ToString()
                    },
                    {ForosPlane.ANSWER_1, "Refuse to see the plotters, fly to Moscow with Rutskoy"},
                    {ForosPlane.ANSWER_2, "Receive Kryuchkov and Yazov and announce their arrest"},
                    {ForosPlane.RESULT_1, new StringBuilder().
                        Append("You refuse to speak with the GKChP members and on the night of August 22 land at Vnukovo ").
                        Append("aboard the Russian leadership's plane. The country will remember Yeltsin as the president's liberator.").
                        ToString()
                    },
                    {ForosPlane.RESULT_2, new StringBuilder().
                        Append("You accept the plotters' surrender yourself: Kryuchkov is arrested on board, Yazov ").
                        Append("admits defeat. The Union president returns to Moscow master of the situation — at least for a while.").
                        ToString()
                    },
                    {ForosPlane.ULTIMATUM + ForosPlane.TITLE, "Ultimatum"},
                    {ForosPlane.ULTIMATUM + ForosPlane.DESCRIPTION, new StringBuilder().
                        Append("On August 21 the GKChP held Moscow: the White House is blockaded, the republics are waiting to see. Kryuchkov and Yazov ").
                        Append("fly to Foros with a ready-made statement of the president's resignation for health reasons. ").
                        Append("“Sign it, Mikhail Sergeyevich, and we guarantee the safety of you and your family.”").
                        ToString()
                    },
                    {ForosPlane.ULTIMATUM + ForosPlane.ANSWER_1, "Sign the resignation statement"},
                    {ForosPlane.ULTIMATUM + ForosPlane.ANSWER_2, "Refuse and demand that the Congress be convened"},
                    {ForosPlane.ULTIMATUM + ForosPlane.RESULT_1, "You sign. The era of perestroika is over."},
                    {ForosPlane.ULTIMATUM + ForosPlane.RESULT_2, new StringBuilder().
                        Append("You refuse to sign and demand that the Congress of People's Deputies be convened. ").
                        Append("The plotters leave, but your fate will now be decided without you.").
                        ToString()
                    }
                }
            },
            {
                ForosReturn.ID, new Dictionary<int, string>()
                {
                    {ForosReturn.TITLE, "Return from Foros"},
                    {ForosReturn.DESCRIPTION, new StringBuilder().
                        Append("On August 22 you are back in the Kremlin. The GKChP members have been arrested; Interior Minister Pugo has shot himself. ").
                        Append("The people who betrayed the president at the head of the army, the KGB, the Interior Ministry and the government must be replaced at once.").
                        ToString()
                    },
                    {ForosReturn.ANSWER_1, "Form a new leadership"},
                    {ForosReturn.RESULT_1, new StringBuilder().
                        Append("Marshal Shaposhnikov is appointed Defense Minister, Bakatin KGB Chairman, ").
                        Append("Barannikov Interior Minister, and Silaev's committee runs the national economy until the end of the year. ").
                        Append("But you have returned to a different country: Yeltsin emerged as the victor of August, ").
                        Append("and one after another the republics declare independence.").
                        ToString()
                    },
                    {ForosReturn.RESIGNATION + ForosReturn.TITLE, "Resignation"},
                    {ForosReturn.RESIGNATION + ForosReturn.DESCRIPTION, new StringBuilder().
                        Append("On August 22 the Supreme Soviet, chaired by Lukyanov, approves the transfer of powers to Yanayev. ").
                        Append("Your refusal no longer changes anything: your guards have been replaced, the dacha is surrounded, and the world accepts the new government ").
                        Append("as a fait accompli.").
                        ToString()
                    },
                    {ForosReturn.RESIGNATION + ForosReturn.ANSWER_1, "Relinquish your powers"},
                    {ForosReturn.RESIGNATION + ForosReturn.RESULT_1, "The President of the USSR has been removed from power."}
                }
            }
        };

        private static Dictionary<int, Dictionary<int, string>> EndingsTextEn = new Dictionary<int, Dictionary<int, string>>()
        {
            {
                LiberalVictory.ID, new Dictionary<int, string>()
                {
                    {LiberalVictory.TITLE, "Victory of the Liberal Forces"},
                    {LiberalVictory.DESCRIPTION, new StringBuilder().
                        Append("You failed to win the support of the Soviet people and ").
                        Append("the state institutions, and did not crush the opposition. As a result, ").
                        Append("the collegiums of the Ministry of Defense and the KGB, the Committee for Constitutional ").
                        Append("Supervision, the Secretariat of the CPSU Central Committee, and then foreign ").
                        Append("states announced that they did not recognize the GKChP; the GKChP failed and was dissolved under the pressure ").
                        Append("of circumstances. The GKChP leaders were arrested, and Pugo took ").
                        Append("his own life, unable to bear the betrayal of his attempts to save the country. ").
                        Append("Marshal Akhromeyev and CPSU Central Committee property manager Kruchina also took ").
                        Append("their own lives, unwilling to witness the collapse of socialism. Gorbachev and his ").
                        Append("closest associates blocked Lukyanov and the Supreme ").
                        Append("Soviet in their attempt to preserve the USSR, dissolving the CPSU Central Committee, the Union ").
                        Append("government and parliament. The signing of an even more ").
                        Append("confederal new Union Treaty was then thwarted by ").
                        Append("the Belovezha conspiracy, at which the leaders of Ukraine, Russia and Belarus ").
                        Append("unconstitutionally declared the USSR dissolved. Throughout 1992 ").
                        Append("the Union institutions were dismantled, a rout crowned by the May Day massacre in Moscow.").
                        ToString()
                    },
                    {LiberalVictory.YELTSIN_ALIVE, "<br>And in 1993 Yeltsin finished off the remnants of Soviet power by shelling the House of Soviets."}
                }
            },
            {
                GorbachevResignation.ID, new Dictionary<int, string>()
                {
                    {GorbachevResignation.TITLE, "Resignation for Health Reasons"},
                    {GorbachevResignation.DESCRIPTION, new StringBuilder().
                        Append("The State Emergency Committee (GKChP) held on to power. The President of the USSR ").
                        Append("resigned “for health reasons” and remained under guard at his dacha in Crimea. ").
                        Append("The Novo-Ogaryovo treaty was never signed, and the democratic opposition was crushed. ").
                        Append("Perestroika, which you began in 1985, ended the same way as Khrushchev's Thaw: ").
                        Append("with a quiet resignation and a long silence. The fate of the Union will be decided without you.").
                        ToString()
                    }
                }
            }
        };
    }
}
