using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using GuTenTak.Ezreal;
using SharpDX;

namespace GuTenTak.Ezreal
{
    internal class Program
    {
        public const string ChampionName = "Ezreal";
        public static Menu Menu, ModesMenu1, ModesMenu2, ModesMenu3, DrawMenu;
        public static AIHeroClient PlayerInstance
        {
            get { return Player.Instance; }
        }
        private static float HealthPercent()
        {
            return (PlayerInstance.Health / PlayerInstance.MaxHealth) * 100;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool AutoQ { get; protected set; }
        public static float Manaah { get; protected set; }

        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Game_OnStart;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Game.OnTick += OnTick;
        }


        static void Game_OnStart(EventArgs args)
        {

            try
            {
                if (ChampionName != PlayerInstance.BaseSkinName)
                {
                    return;
                }

                Q = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, 250, 2000, 60);
                Q.AllowedCollisionCount = 0;
                W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Linear, 250, 1600, 80);
                W.AllowedCollisionCount = int.MaxValue;
                E = new Spell.Skillshot(SpellSlot.E, 475, SkillShotType.Linear);
                E.AllowedCollisionCount = int.MaxValue;
                R = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear, 1000, 2000, 160);
                R.AllowedCollisionCount = int.MaxValue;



                Bootstrap.Init(null);
                Chat.Print("GuTenTak Addon Loading Success", Color.Green);


                Menu = MainMenu.AddMenu("GuTenTak Ezreal", "Ezreal");
                Menu.AddSeparator();
                Menu.AddLabel("GuTenTak Ezreal Addon");

                var Enemies = EntityManager.Heroes.Enemies.Where(a => !a.IsMe).OrderBy(a => a.BaseSkinName);
                ModesMenu1 = Menu.AddSubMenu("Menu", "Modes1Ezreal");
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("Combo Configs");
                ModesMenu1.Add("ComboQ", new CheckBox("Use Q on Combo", true));
                ModesMenu1.Add("ComboW", new CheckBox("Use W on Combo", true));
                ModesMenu1.Add("ManaCW", new Slider("Use W Mana %", 30));
                ModesMenu1.Add("ComboR", new CheckBox("Use R on Combo", true));
                ModesMenu1.Add("RCount", new Slider("Cast R if Will Hit >=", 3, 2, 5));
                ModesMenu1.AddSeparator();
                //ModesMenu1.Add("useItem", new CheckBox("Use Items on Combo", true));
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("AutoHarass Configs");
                ModesMenu1.Add("AutoHarass", new CheckBox("Use Q on AutoHarass", true));
                ModesMenu1.Add("ManaAuto", new Slider("Mana %", 80));
                ModesMenu1.AddLabel("Harass Configs");
                ModesMenu1.Add("ManaH", new Slider("Harass Mana %", 40));
                ModesMenu1.Add("HarassQ", new CheckBox("Use Q on Harass", true));
                ModesMenu1.Add("HarassW", new CheckBox("Use W on Harass", true));
                ModesMenu1.Add("ManaHW", new Slider("Mana %", 60));
                ModesMenu1.AddSeparator();
                ModesMenu1.AddLabel("Kill Steal Configs");
                ModesMenu1.Add("KQ", new CheckBox("Use Q on KillSteal", true));
                ModesMenu1.Add("KW", new CheckBox("Use W on KillSteal", true));
                ModesMenu1.Add("KR", new CheckBox("Use R on KillSteal", true));

                ModesMenu2 = Menu.AddSubMenu("Farm", "Modes2Ezreal");
                ModesMenu2.AddLabel("LastHit Configs");
                ModesMenu2.Add("ManaF", new Slider("Mana %", 60));
                ModesMenu2.Add("LastQ", new CheckBox("Use Q on LastHit", true));
                ModesMenu2.AddLabel("Lane Clear Config");
                ModesMenu2.Add("ManaL", new Slider("Mana %", 40));
                ModesMenu2.Add("FarmQ", new CheckBox("Use Q on LaneClear", true));
                ModesMenu2.AddLabel("Jungle Clear Config");
                ModesMenu2.Add("ManaJ", new Slider("Mana %", 40));
                ModesMenu2.Add("JungleQ", new CheckBox("Use Q on JungleClear", true));

                ModesMenu3 = Menu.AddSubMenu("Misc", "Modes3Ezreal");
                ModesMenu3.AddLabel("Misc Configs");
                ModesMenu3.Add("AntiGap", new CheckBox("Use E for Anti-Gapcloser", true));
                // ModesMenu3.Add("Flee", new KeyBind("Flee", false, KeyBind.BindTypes.HoldActive, "G".ToCharArray()[0]));
                ModesMenu3.AddLabel("Flee Configs");
                ModesMenu3.Add("FleeQ", new CheckBox("Use Q on Flee", true));
                ModesMenu3.Add("FleeE", new CheckBox("Use E on Flee", true));
                ModesMenu3.Add("ManaFlQ", new Slider("Q Mana %", 60));

                DrawMenu = Menu.AddSubMenu("Draws", "DrawEzreal");
                DrawMenu.Add("drawQ", new CheckBox(" Draw Q", true));
                DrawMenu.Add("drawW", new CheckBox(" Draw W", true));
                DrawMenu.Add("drawR", new CheckBox(" Draw R", false));
                DrawMenu.Add("drawXR", new CheckBox(" Draw Don't Use R", true));
                DrawMenu.Add("drawXFleeQ", new CheckBox(" Draw Don't Use Flee Q", false));

            }

            catch (Exception e)
            {
                Chat.Print("GuTenTak.Ezreal: Exception occured while Initializing Addon. Error: " + e.Message);

            }

        }
        private static void Game_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                if (Q.IsReady() && Q.IsLearned)
                {
                    Circle.Draw(Color.White, Q.Range, Player.Instance.Position);
                }
            }
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                if (W.IsReady() && W.IsLearned)
                {
                    Circle.Draw(Color.White, W.Range, Player.Instance.Position);
                }
            }
            if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                if (R.IsReady() && R.IsLearned)
                {
                    Circle.Draw(Color.White, R.Range, Player.Instance.Position);
                }
            }
            if (DrawMenu["drawXR"].Cast<CheckBox>().CurrentValue)
            {
                if (R.IsReady() && R.IsLearned)
                {
                    Circle.Draw(Color.Red, 700, Player.Instance.Position);
                }
            }
            if (DrawMenu["drawXFleeQ"].Cast<CheckBox>().CurrentValue)
            {
                if (Q.IsReady() && Q.IsLearned)
                {
                    Circle.Draw(Color.Red, 400, Player.Instance.Position);
                }
            }

            if (R.IsReady() && R.IsLearned)
            {
                Circle.Draw(Color.Black, R.Range, Player.Instance.Position);
            }

        }
        static void Game_OnUpdate(EventArgs args)
        {
            var AutoHQ = ModesMenu1["AutoHarass"].Cast<CheckBox>().CurrentValue;
            var ManaAuto = ModesMenu1["ManaAuto"].Cast<Slider>().CurrentValue;
            if (AutoHQ && ManaAuto <= _Player.ManaPercent)
            {
                    //Thanks.KEzreal
                    Common.AutoQ();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Common.Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Common.Harass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {

                Common.LaneClear();

            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {

                Common.JungleClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                Common.LastHit();

            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Common.Flee();

            }
        }
        public static void OnTick(EventArgs args)
        {


            Common.KillSteal();

        }
    }
}