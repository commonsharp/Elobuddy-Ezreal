using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace GuTenTak.Ezreal
{
    internal class Common : Program
    {
        public static Obj_AI_Base GetFindObj(Vector3 Pos, string name, float range)
        {
            var CusPos = Pos;
            {
                var GetObj = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(f => f.IsAlly && !f.IsMe && f.Position.Distance(ObjectManager.Player.Position) < range && f.Distance(CusPos) < 150);
                if (GetObj != null)
                    return GetObj;
                return null;
            }
        }

        public static void MovingPlayer(Vector3 Pos)
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Pos);
        }
        public static Vector2 ToScreen(Vector3 Target)
        {
            var target = Drawing.WorldToScreen(Target);
            return target;
        }

        public static void Combo()
        {

            var Target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (Target == null) return;
            var useQ = ModesMenu1["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ModesMenu1["ComboW"].Cast<CheckBox>().CurrentValue;
            var useR = ModesMenu1["ComboR"].Cast<CheckBox>().CurrentValue;
            var Qp = Q.GetPrediction(Target);
            var Wp = W.GetPrediction(Target);
            var Rp = R.GetPrediction(Target);
            if (!Target.IsValid()) return;
            //if (ModesMenu1["useItem"].Cast<CheckBox>().CurrentValue)
            // {
            //Itens.useItemtens();
            //}


            if (Q.IsInRange(Target) && Q.IsReady() && useQ && Qp.HitChance >= HitChance.High)
            {
                Q.Cast(Qp.CastPosition);
            }
            if (W.IsInRange(Target) && W.IsReady() && useW && Wp.HitChance >= HitChance.High)
            {
                W.Cast(Wp.CastPosition);

            }
            if (R.IsInRange(Target) && R.IsReady() && useR)
            {
                if (ObjectManager.Player.CountEnemiesInRange(700) == 0)
                {//Thanks to Hi I'm Ezreal
                    foreach (var hero in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(3000)))
                    {
                        if (R.IsReady())
                        {
                            var collision = new List<AIHeroClient>();
                            var startPos = Player.Instance.Position.To2D();
                            var endPos = hero.Position.To2D();
                            collision.Clear();
                            foreach (
                                var colliHero in
                                    EntityManager.Heroes.Enemies.Where(
                                        colliHero =>
                                            !colliHero.IsDead && colliHero.IsVisible &&
                                            colliHero.IsInRange(hero, 3000) && colliHero.IsValidTarget(3000)))
                            {
                                if (Prediction.Position.Collision.LinearMissileCollision(colliHero, startPos, endPos,
                                    R.Speed, R.Width, R.CastDelay))
                                {
                                    collision.Add(colliHero);
                                }

                                var RTargets = Program.ModesMenu1["RCount"].Cast<Slider>().CurrentValue;
                                if (collision.Count >= RTargets)
                                {
                                    R.Cast(hero);
                                }
                                //
                            }
                        }
                    }
                }
            }
        }

        public static void Harass()
        {
            //Harass

            var Target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (Target == null) return;
            var TargetR = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            var useQ = ModesMenu1["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = ModesMenu1["HarassW"].Cast<CheckBox>().CurrentValue;
            var Qp = Q.GetPrediction(Target);
            var Wp = W.GetPrediction(Target);
            if (!Target.IsValid() && Target == null) return;


            if (Q.IsInRange(Target) && Q.IsReady() && useQ && Qp.HitChance >= HitChance.High)
            {
                Q.Cast(Qp.CastPosition);
            }
            if (W.IsInRange(Target) && W.IsReady() && useW && Wp.HitChance >= HitChance.High && Program._Player.ManaPercent <= Program.ModesMenu2["ManaHW"].Cast<Slider>().CurrentValue)
            {
                W.Cast(Wp.CastPosition);

            }
        }
        public static void LaneClear()
        {
            bool lastQ = false;
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                Player.Instance.ServerPosition, Q.Range).OrderBy(h => h.Health);
            {
                if (minions.Any() && !lastQ && Program._Player.ManaPercent >= Program.ModesMenu2["ManaL"].Cast<Slider>().CurrentValue) 
                {
                    var getHealthyCs = minions.GetEnumerator();
                    while (getHealthyCs.MoveNext())
                    {
                        Q.Cast(Q.GetPrediction(minions.Last()).CastPosition);
                    }
                }
            }
        }

      //    var useQ = ModesMenu2["FarmQ"].Cast<CheckBox>().CurrentValue;
    //      var minions = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(Q.Range));
    //      var minion = EntityManager.MinionsAndMonsters.EnemyMinions.Where(t => t.IsInRange(Player.Instance.Position, W.Range) && !t.IsDead && t.IsValid && !t.IsInvulnerable).Count();
    //      if (minions == null) return;
    //      if ((_Player.ManaPercent <= Program.ModesMenu2["ManaF"].Cast<Slider>().CurrentValue))
    //      {
    //          return;
    //      }
    //
    //      if (useQ && Q.IsReady() && Q.IsInRange(minions))
    //      {
    //          Q.Cast(minions);
    //      }
    //
     // }
        public static void JungleClear()
        {

            var useQ = ModesMenu2["JungleQ"].Cast<CheckBox>().CurrentValue;
            var jungleMonsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Program.Q.Range));
            var minioon = EntityManager.MinionsAndMonsters.EnemyMinions.Where(t => t.IsInRange(Player.Instance.Position, Program.Q.Range) && !t.IsDead && t.IsValid && !t.IsInvulnerable).Count();
            if (jungleMonsters == null) return;
            if ((Program._Player.ManaPercent <= Program.ModesMenu2["ManaJ"].Cast<Slider>().CurrentValue))
            {
                return;
            }
            var Qp = Q.GetPrediction(jungleMonsters);
            if (jungleMonsters == null) return;
            if (useQ && Q.IsReady() && Q.IsInRange(jungleMonsters))
            {
                Q.Cast(Qp.CastPosition);
            }

        }

        public static void LastHit()
        {

            var useQ = Program.ModesMenu2["LastQ"].Cast<CheckBox>().CurrentValue;
            var qminions = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget((Program.Q.Range)) && (DamageLib.QCalc(m) > m.Health));
            if (qminions == null) return;
            if ((Program._Player.ManaPercent <= Program.ModesMenu2["ManaF"].Cast<Slider>().CurrentValue))
            {
                return;
            }

            if (Q.IsReady() && (Program._Player.Distance(qminions) <= Program._Player.GetAutoAttackRange()) && useQ && qminions.Health < DamageLib.QCalc(qminions))
            {
                Q.Cast(qminions);
            }

        }

        public static void Flee()
        {
            if (ModesMenu3["FleeE"].Cast<CheckBox>().CurrentValue)
            {
                var tempPos = Game.CursorPos;
                if (tempPos.IsInRange(Player.Instance.Position, E.Range))
                {
                    E.Cast(tempPos);
                }
                else
                {
                    E.Cast(Player.Instance.Position.Extend(tempPos, 450).To3DWorld());
                }
            }
            if (ModesMenu3["FleeQ"].Cast<CheckBox>().CurrentValue && Program._Player.ManaPercent <= Program.ModesMenu3["ManaFlQ"].Cast<Slider>().CurrentValue)
            {
                if (ObjectManager.Player.CountEnemiesInRange(400) == 0)
                {
                    var Target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                    if (Target == null) return;
                    var Qp = Q.GetPrediction(Target);
                    Q.Cast(Qp.CastPosition);
                }

            }
        }

        public static void KillSteal()
        {


            foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => !a.IsDead && !a.IsZombie && a.Health > 0))
            {
                if (enemy == null) return;


                if (enemy.IsValidTarget(R.Range) && enemy.HealthPercent <= 40)
                {

                    if (DamageLib.QCalc(enemy) + DamageLib.WCalc(enemy) + DamageLib.RCalc(enemy) >= enemy.Health)
                    {
                        var Qp = Q.GetPrediction(enemy);
                        var Wp = W.GetPrediction(enemy);
                        var Ep = E.GetPrediction(enemy);
                        var Rp = R.GetPrediction(enemy);
                        if (Q.IsReady() && Q.IsInRange(enemy) && Program.ModesMenu1["KQ"].Cast<CheckBox>().CurrentValue && Qp.HitChancePercent >= 90)
                        {
                            Q.Cast(Qp.CastPosition);
                        }
                        if (W.IsReady() && W.IsInRange(enemy) && Program.ModesMenu1["KW"].Cast<CheckBox>().CurrentValue && Wp.HitChancePercent >= 90)
                        {
                            W.Cast(Wp.CastPosition);
                        }
                        if (R.IsReady() && R.IsInRange(enemy) && Program.ModesMenu1["KR"].Cast<CheckBox>().CurrentValue && Rp.HitChancePercent >= 90)
                        {
                            if (ObjectManager.Player.CountEnemiesInRange(700) == 0)
                            {
                                R.Cast(Rp.CastPosition);
                            }
                        }
                    }
                }
            }
        }

        //    public static new void AutoQ()
        //     {
        //        var alvo = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
        //         if (alvo == null) return;
        //        var useQ = ModesMenu1["AutoHarass"].Cast<CheckBox>().CurrentValue;
        //         var Qp = Q.GetPrediction(alvo);
        //        if (!alvo.IsValid()) return;
        //        if (Q.IsInRange(alvo) && Q.IsReady() && useQ && Qp.HitChance >= HitChance.High)
        //        {
        //             Q.Cast(Qp.CastPosition);
        //         }
        //    }


        public static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (Program.ModesMenu2["AntiGap"].Cast<CheckBox>().CurrentValue)
                {
                string[] herogapcloser =
                {
                "Braum", "Ekko", "Elise", "Fiora", "Kindred", "Lucian", "Yi", "Nidalee", "Quinn", "Riven", "Shaco", "Sion", "Vayne", "Yasuo", "Graves", "Azir", "Gnar", "Irelia", "Kalista"
            };
                if (sender.IsEnemy && sender.GetAutoAttackRange() >= ObjectManager.Player.Distance(gapcloser.End) && !herogapcloser.Any(sender.ChampionName.Contains))
                {
                    var diffGapCloser = gapcloser.End - gapcloser.Start;
                    E.Cast(ObjectManager.Player.ServerPosition + diffGapCloser);
                }
            }
        }
    }
}