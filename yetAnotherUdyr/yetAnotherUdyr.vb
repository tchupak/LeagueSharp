﻿Imports LeagueSharp
Imports LeagueSharp.Common
Imports LeagueSharp.Common.TargetSelector
Imports SharpDX
Imports System.Drawing
Imports System.Collections.Generic

Public Class yetAnotherUdyr

	'Script Information
	Shared versionNumber As String = "1.0.7"

	'Ease of use
	Shared player As Obj_AI_Hero = ObjectManager.Player

	Shared Config As Menu
	Shared Orbwalker As Orbwalking.Orbwalker

	Shared levelUpListPhoenix As New List(Of Integer) From {3, 2, 1, 0, 3, 3, 1, 3, 1, 3, 1, 1, 2, 2, 2, 2, 0, 0}
	Shared levelUpListTiger As New List(Of Integer) From {0, 2, 1, 0, 0, 2, 0, 1, 0, 2, 2, 2, 1, 1, 1, 3, 3, 3}

	'Spells
	Shared Q As Spell
	Shared W As Spell
	Shared E As Spell
	Shared R As Spell

	Shared spellList As New List(Of Spell)

	'ignite
	Shared igniteSpell As SpellSlot
	Shared hasIgnite As Boolean
	Shared ignite As New Igniter.Ignite

	'smite
	Shared smiteSpell As SpellSlot
	Shared hasSmite As Boolean
	Shared smiteList As New List(Of String) From {"Dragon", "Worm", "AncientGolem", "LizardElder"}
	'Items
	''Offensive - minus 25 range
	Shared BilgeCut As New Items.Item(3144, 475)
	Shared BoTRK As New Items.Item(3153, 425)
	Shared RavHydra As New Items.Item(3074, 375)
	Shared Tiamat As New Items.Item(3077, 375)
	''Defensive - minus 10 range
	Shared LoTIS As New Items.Item(3190, 590)
	Shared RanOmen As New Items.Item(3143, 490)

	'Drawing
	Shared enemyColor As New Dictionary(Of Integer, System.Drawing.Color)
	Shared enemyText As New Dictionary(Of Integer, String)

	Public Shared Sub Main(ByVal arg() As String)
		AddHandler CustomEvents.Game.OnGameLoad, AddressOf Game_onGameLoad

	End Sub

#Region "Event Handlers"
	Public Shared Sub Game_onGameLoad(args As System.EventArgs)
		Try
			Console.WriteLine("1")

			Game.PrintChat("yetAnotherUdyr by FlapperDoodle, version: " & versionNumber)
			If Not ObjectManager.Player.ChampionName = "Udyr" Then
				Game.PrintChat("Please use Udyr~")
				Return
			End If

			'Spell Initialize
			Q = New Spell(SpellSlot.Q, 200)
			W = New Spell(SpellSlot.W, 200)
			E = New Spell(SpellSlot.E, 200)
			R = New Spell(SpellSlot.R, 200)


			'Main Menu
			Config = New Menu("yA-Udyr", "yA-Udyr", True)


			'Orbwalker
			Config.AddSubMenu(New Menu("Orbwalking", "Orbwalking"))
			Orbwalker = New Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"))


			'Target Selector
			Dim TargetSelector = New Menu("Target Selector", "Target Selector")
			SimpleTs.AddToMenu(TargetSelector)
			Config.AddSubMenu(TargetSelector)


			'Main
			Config.AddItem(New MenuItem("Combo Key", "Combo Key").SetValue(New KeyBind(32, KeyBindType.Press)))
			Config.AddItem(New MenuItem("Style", "Style").SetValue(New StringList({"Phoenix", "Tiger"}, 0)))

			'Items
			Config.AddSubMenu(New Menu("Items", "Items"))
			''Offensive
			Config.SubMenu("Items").AddSubMenu(New Menu("Offense", "Offense"))
			Config.SubMenu("Items").SubMenu("Offense").AddItem(New MenuItem("BilgeCut", "Bilgewater Cutlass").SetValue(True))
			Config.SubMenu("Items").SubMenu("Offense").AddItem(New MenuItem("BoTRK", "BoT Ruined King").SetValue(True))
			Config.SubMenu("Items").SubMenu("Offense").AddItem(New MenuItem("RavHydra", "Ravenous Hydra").SetValue(True))
			Config.SubMenu("Items").SubMenu("Offense").AddItem(New MenuItem("RanOmen", "Randuin's Omen").SetValue(True))
			Config.SubMenu("Items").SubMenu("Offense").AddItem(New MenuItem("Tiamat", "Tiamat").SetValue(True))

			''Defensive
			Config.SubMenu("Items").AddSubMenu(New Menu("Defense", "Defense"))
			Config.SubMenu("Items").SubMenu("Defense").AddSubMenu(New Menu("LoT Iron Solari", "LoTIS-Menu"))
			'''LoT-IS
			Config.SubMenu("Items").SubMenu("Defense").SubMenu("LoTIS-Menu").AddItem(New MenuItem("LoTIS", "Enabled").SetValue(True))
			Config.SubMenu("Items").SubMenu("Defense").SubMenu("LoTIS-Menu").AddItem(New MenuItem("LoTIS-HP-%", "Use at HP %").SetValue(New Slider(40)))

			'Farm
			Config.AddSubMenu(New Menu("Farm", "Farm"))
			Config.SubMenu("Farm").AddItem(New MenuItem("Use-Q-Farm", "Use Q").SetValue(True))
			Config.SubMenu("Farm").AddItem(New MenuItem("Use-R-Farm", "Use R").SetValue(True))
			Config.SubMenu("Farm").AddItem(New MenuItem("Farm-Mana", "Mana Limit").SetValue(New Slider(20)))
			Config.SubMenu("Farm").AddItem(New MenuItem("Farm Key", "Farm Key").SetValue(New KeyBind(86, KeyBindType.Press)))

			'Jungle Farm
			Config.AddSubMenu(New Menu("Jungle Farm", "Jungle Farm"))
			Config.SubMenu("Jungle Farm").AddItem(New MenuItem("Use-Q-Jungle", "Use Q").SetValue(True))
			Config.SubMenu("Jungle Farm").AddItem(New MenuItem("Use-R-Jungle", "Use R").SetValue(True))
			Config.SubMenu("Jungle Farm").AddItem(New MenuItem("Use-W-Jungle", "Use W").SetValue(True))
			Config.SubMenu("Jungle Farm").AddItem(New MenuItem("Jungle-Mana", "Mana Limit").SetValue(New Slider(20)))
			Config.SubMenu("Jungle Farm").AddItem(New MenuItem("Jungle Farm Key", "Jungle Farm Key").SetValue(New KeyBind(67, KeyBindType.Press)))

			'Misc
			Config.AddSubMenu(New Menu("Misc", "Misc"))
			Config.SubMenu("Misc").AddItem(New MenuItem("Auto Level", "Auto Level").SetValue(True))
			Config.SubMenu("Misc").AddItem(New MenuItem("Stun Lock", "Stun Lock").SetValue(True))

			''Ignite
			igniteSpell = player.GetSpellSlot("SummonerDot")

			If igniteSpell = SpellSlot.Unknown Then
				Game.PrintChat("yA-Udyr: Ignite-related functions disabled.")
			Else
				Config.SubMenu("Misc").AddSubMenu(New Menu("Ignite", "Ignite"))
				Config.SubMenu("Misc").SubMenu("Ignite").AddItem(New MenuItem("Auto Ignite", "Auto Ignite").SetValue(New StringList({"In Combo", "Killsteal", "Killable", "Never"}, 0)))
				Config.SubMenu("Misc").SubMenu("Ignite").AddItem(New MenuItem("Ignite HP", "Ignite in Combo, HP %").SetValue(New Slider(35)))
				AddHandler ignite.CanKillEnemies, AddressOf CanKillEnemiesIgnite
				AddHandler ignite.CanKillstealEnemies, AddressOf CanKillStealEnemiesIgnite
				hasIgnite = True
			End If

			''Smite
			smiteSpell = player.GetSpellSlot("SummonerSmite")

			If smiteSpell = SpellSlot.Unknown Then
				Game.PrintChat("yA-Udyr: Smite-related functions disabled.")
			Else
				Config.SubMenu("Misc").AddSubMenu(New Menu("Smite", "Smite"))
				Config.SubMenu("Misc").SubMenu("Smite").AddItem(New MenuItem("Auto Smite", "Auto Smite").SetValue(New StringList({"Buffs Only", "D+B Only", "Both", "Never"}, 2)))
				hasSmite = True
			End If


			'Drawing
			Config.AddSubMenu(New Menu("Drawing", "Drawing"))
			Config.SubMenu("Drawing").AddItem(New MenuItem("Draw", "Draw").SetValue(True))
			''Enemy Status
			For Each enemy As Obj_AI_Hero In ObjectManager.Get(Of Obj_AI_Hero)()
				If enemy.IsEnemy Then
					enemyColor.Add(enemy.NetworkId, System.Drawing.Color.Green)
					Config.SubMenu("Drawing").AddSubMenu(New Menu(enemy.ChampionName, enemy.ChampionName))
					Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(New MenuItem(enemy.NetworkId & "E", "Enabled").SetValue(True))
					Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(New MenuItem(enemy.NetworkId & "KC", "Killable Circle").SetValue(True))
					Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(New MenuItem(enemy.NetworkId & "HP", "HP").SetValue(True))
					Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(New MenuItem(enemy.NetworkId & "MP", "MP").SetValue(True))
					Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(New MenuItem(enemy.NetworkId & "R", "Range").SetValue(New StringList({"Basic Attack", "Q", "W", "E", "R", "None"})))
				End If
			Next
			Config.AddToMainMenu()

			'Handles
			AddHandler Game.OnGameUpdate, AddressOf Game_OnGameUpdate
			AddHandler Drawing.OnDraw, AddressOf Drawing_OnDraw
			AddHandler CustomEvents.Unit.OnLevelUp, AddressOf Unit_OnLevelUp
			AddHandler Orbwalking.BeforeAttack, AddressOf Orbwalking_BeforeAttack

		Catch ex As Exception
			Console.WriteLine(ex.ToString())
		End Try


	End Sub

	'Orbwalk Events Here, farm goes here
	Shared Sub Orbwalking_BeforeAttack(args As Orbwalking.BeforeAttackEventArgs)
		'if farm key is pressed
		If Config.Item("Farm Key").GetValue(Of KeyBind).Active AndAlso ((player.Mana / player.MaxMana) * 100) >= Config.Item("Farm-Mana").GetValue(Of Slider).Value Then
			Farm()
		End If
	End Sub
	'Auto level up skill, Andi needs to fix this >.>
	Shared Sub Unit_OnLevelUp(sender As Obj_AI_Base, args As CustomEvents.Unit.OnLevelUpEventArgs)
		If sender.IsMe AndAlso Config.Item("Auto Level").GetValue(Of Boolean)() Then
			If Config.Item("Style").GetValue(Of StringList)().SelectedIndex = 0 Then
				player.Spellbook.LevelUpSpell(levelUpListPhoenix(args.NewLevel - 1))
			Else
				player.Spellbook.LevelUpSpell(levelUpListTiger(args.NewLevel - 1))
			End If
		End If
	End Sub
	'If KillableEnemies are in range and the config are on
	Shared Sub CanKillEnemiesIgnite(sender As Object, args As Igniter.IgniteEventArgs)
		If Config.Item("Auto Ignite").GetValue(Of StringList)().SelectedIndex = 2 Then
			'ignite first killable enemy
			ignite.Cast(args.Enemies(0))
		End If
	End Sub
	Shared Sub CanKillStealEnemiesIgnite(sender As Object, args As Igniter.IgniteEventArgs)
		If Config.Item("Auto Ignite").GetValue(Of StringList)().SelectedIndex = 1 Or Config.Item("Auto Ignite").GetValue(Of StringList)().SelectedIndex = 2 Then
			ignite.Cast(args.Enemies(0))
		End If
	End Sub

	Shared Sub Game_OnGameUpdate(args As EventArgs)
		'If combo key is pressed
		If Config.Item("Combo Key").GetValue(Of KeyBind).Active Then
			comboIt()
		End If

		'Smite
		If hasSmite = True AndAlso Not Config.Item("Auto Smite").GetValue(Of StringList)().SelectedIndex = 3 AndAlso player.SummonerSpellbook.CanUseSpell(smiteSpell) = SpellState.Ready Then
			castSmite()
		End If

		'Defensive Items
		If Config.Item("LoTIS").GetValue(Of Boolean)() AndAlso LoTIS.IsReady Then
			If ((player.Health / player.MaxHealth) * 100) <= Config.Item("LoTIS-HP-%").GetValue(Of Slider).Value() Then
				LoTIS.Cast(player)
			End If
		End If

		'If jungle key is pressed
		If Config.Item("Jungle Farm Key").GetValue(Of KeyBind).Active AndAlso ((player.Mana / player.MaxMana) * 100) >= Config.Item("Jungle-Mana").GetValue(Of Slider).Value Then
			jungleFarm()
		End If

		'If drawing is on
		If Config.Item("Draw").GetValue(Of Boolean)() Then
			updateIsKillable()
		End If
	End Sub
#End Region
#Region "Drawing"
	Shared Sub Drawing_OnDraw(args As EventArgs)
		If Config.Item("Draw").GetValue(Of Boolean)() Then
			For Each enemyVisible As Obj_AI_Hero In ObjectManager.Get(Of Obj_AI_Hero)()
				If enemyVisible.IsValidTarget Then

					If Config.Item(enemyVisible.NetworkId & "E").GetValue(Of Boolean)() Then

						If Config.Item(enemyVisible.NetworkId & "KC").GetValue(Of Boolean)() Then
							Utility.DrawCircle(enemyVisible.Position, 60, enemyColor(enemyVisible.NetworkId), 2, 15)
						End If
						If Config.Item(enemyVisible.NetworkId & "HP").GetValue(Of Boolean)() Then
							Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)(0) - 40, Drawing.WorldToScreen(enemyVisible.Position)(1) - 100, System.Drawing.Color.Red, Convert.ToInt32(enemyVisible.Health / enemyVisible.MaxHealth * 100) & "%")
						End If
						If Config.Item(enemyVisible.NetworkId & "MP").GetValue(Of Boolean)() Then
							Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)(0) + 10, Drawing.WorldToScreen(enemyVisible.Position)(1) - 100, System.Drawing.Color.BlueViolet, Convert.ToInt32(enemyVisible.Mana / enemyVisible.MaxMana * 100) & "%")
						End If
						Select Case Config.Item(enemyVisible.NetworkId & "R").GetValue(Of StringList).SelectedIndex
							Case 5
								Exit Select
							Case 0
								Utility.DrawCircle(enemyVisible.Position, enemyVisible.BasicAttack.CastRadius(0), System.Drawing.Color.White, 1, 25)
							Case 1
								Utility.DrawCircle(enemyVisible.Position, enemyVisible.Spellbook.GetSpell(SpellSlot.Q).SData.CastRadius(0), System.Drawing.Color.White, 1, 25)
							Case 2
								Utility.DrawCircle(enemyVisible.Position, enemyVisible.Spellbook.GetSpell(SpellSlot.W).SData.CastRadius(0), System.Drawing.Color.White, 1, 25)
							Case 3
								Utility.DrawCircle(enemyVisible.Position, enemyVisible.Spellbook.GetSpell(SpellSlot.E).SData.CastRadius(0), System.Drawing.Color.White, 1, 25)
							Case 4
								Utility.DrawCircle(enemyVisible.Position, enemyVisible.Spellbook.GetSpell(SpellSlot.R).SData.CastRadius(0), System.Drawing.Color.White, 1, 25)
						End Select
					End If
				End If
			Next
		End If
	End Sub
#End Region

#Region "Methods/Functions"
	Shared Sub comboIt()
		'Create target

		Dim target = SimpleTs.GetTarget(600.0F, SimpleTs.DamageType.Magical)

		If target IsNot Nothing Then

			Dim configIgnite = Config.Item("Auto Ignite").GetValue(Of StringList)().SelectedIndex

			'If auto-ignite is set to "In Combo" or "Both" mode
			If hasIgnite AndAlso player.SummonerSpellbook.CanUseSpell(igniteSpell) = SpellState.Ready Then
				If configIgnite = 2 AndAlso ignite.CanKill(target) Then
					ignite.Cast(target)
				ElseIf configIgnite = 0 AndAlso Config.Item("Ignite HP").GetValue(Of Slider).Value > ((target.Health / target.MaxHealth) * 100) Then
					ignite.Cast(target)
				End If
			End If
			'If stun lock is on, the target doesn't have a stun buff, and the spell is ready, then cast bear stun
			If Config.Item("Stun Lock").GetValue(Of Boolean)() AndAlso E.IsReady() AndAlso Not target.HasBuff("udyrbearstuncheck") AndAlso player.Distance(target) < 200 Then
				E.Cast()
			End If

			'Skill order sequence
			If player.Distance(target) < 300 Then
				If Config.Item("Style").GetValue(Of StringList)().SelectedIndex = 0 Then
					If R.IsReady() Then
						R.Cast()
					End If
					If Q.IsReady() Then
						Q.Cast()
					End If
					If E.IsReady AndAlso Not target.HasBuff("udyrbearstuncheck") Then
						E.Cast()
					End If
					If W.IsReady() Then
						W.Cast()
					End If
				Else
					If E.IsReady AndAlso Not target.HasBuff("udyrbearstuncheck") Then
						E.Cast()
					End If
					If Q.IsReady() Then
						Q.Cast()
					End If
					If W.IsReady() Then
						W.Cast()
					End If
					If R.IsReady() Then
						R.Cast()
					End If
				End If
			End If

			'Do Attack Items
			If Config.Item("RavHydra").GetValue(Of Boolean)() AndAlso RavHydra.IsReady Then
				RavHydra.Cast(target)
			End If
			If Config.Item("BoTRK").GetValue(Of Boolean)() AndAlso BoTRK.IsReady Then
				BoTRK.Cast(target)
			End If
			If Config.Item("BilgeCut").GetValue(Of Boolean)() AndAlso BilgeCut.IsReady Then
				BilgeCut.Cast(target)
			End If
			If Config.Item("RanOmen").GetValue(Of Boolean)() AndAlso RanOmen.IsReady Then
				For Each enemy In ObjectManager.Get(Of Obj_AI_Hero)()
					If Utility.IsValidTarget(enemy, 490) Then
						RanOmen.Cast(player)
					End If
				Next
			End If
			If Config.Item("Tiamat").GetValue(Of Boolean)() AndAlso Tiamat.IsReady Then
				Tiamat.Cast(target)
			End If
		End If
	End Sub

	Shared Sub Farm()
		If Not Orbwalking.CanMove(50) Then Return
		Dim minions = MinionManager.GetMinions(player.ServerPosition, 500.0F)
		If minions.Count > 2 Then
			Dim useR = Config.Item("Use-R-Farm").GetValue(Of Boolean)()
			Dim useQ = Config.Item("Use-Q-Farm").GetValue(Of Boolean)()

			If (useR AndAlso R.IsReady) OrElse (useQ AndAlso Q.IsReady) Then
				For Each minion In minions
					If useQ Then
						Q.Cast()
					End If
					If useR Then
						R.Cast()
					End If
				Next
			End If
		End If
	End Sub
	Shared Sub jungleFarm()

		Dim jungleMobs = MinionManager.GetMinions(player.ServerPosition, 700, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
		If jungleMobs.Count > 0 Then
			If Config.Item("Use-R-Jungle").GetValue(Of Boolean)() AndAlso R.IsReady() Then
				R.Cast()
			End If
			If Config.Item("Use-Q-Jungle").GetValue(Of Boolean)() AndAlso Q.IsReady() Then
				Q.Cast()
			End If
			If Config.Item("Use-W-Jungle").GetValue(Of Boolean)() AndAlso W.IsReady() Then
				W.Cast()
			End If
		End If
	End Sub
	Shared Sub castSmite()
		Dim mobs = MinionManager.GetMinions(player.ServerPosition, 900.0F, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
		If mobs.Count > 0 Then
			Dim smiteDamage = Math.Max(20 * player.Level + 370, Math.Max(30 * player.Level + 330, Math.Max(40 * player.Level + 240, 50 * player.Level + 100)))
			For Each mob In mobs
				If Not smiteList.Contains(mob.SkinName) Then Continue For

				Select Case Config.Item("Auto Smite").GetValue(Of StringList)().SelectedIndex
					Case 0
						If (mob.SkinName = "AncientGolem" OrElse mob.SkinName = "LizardElder") AndAlso mob.IsValidTarget(790) AndAlso smiteDamage >= mob.Health Then
							player.SummonerSpellbook.CastSpell(smiteSpell, mob)
						End If
					Case 1
						If (mob.SkinName = "Dragon" OrElse mob.SkinName = "Worm") AndAlso mob.IsValidTarget(790) AndAlso smiteDamage >= mob.Health Then
							player.SummonerSpellbook.CastSpell(smiteSpell, mob)
							Exit Select
						End If
					Case 2
						If (mob.SkinName = "Dragon" OrElse mob.SkinName = "Worm" OrElse mob.SkinName = "AncientGolem" OrElse mob.SkinName = "LizardElder") AndAlso mob.IsValidTarget(790) AndAlso smiteDamage >= mob.Health Then
							player.SummonerSpellbook.CastSpell(smiteSpell, mob)
							Exit Select
						End If
				End Select
			Next
		End If
	End Sub
	Shared Sub updateIsKillable()

		For Each enemy As Obj_AI_Hero In ObjectManager.Get(Of Obj_AI_Hero)()
			If Utility.IsValidTarget(enemy) Then

				Dim Adamage = DamageLib.getDmg(enemy, DamageLib.SpellType.AD, DamageLib.StageType.Default)
				Dim qDamage = 0
				Dim rDamage = 0

				If Q.IsReady Then qDamage = DamageLib.getDmg(enemy, DamageLib.SpellType.Q, DamageLib.StageType.Default)
				If R.IsReady Then rDamage = DamageLib.getDmg(enemy, DamageLib.SpellType.R, DamageLib.StageType.Default)

				Dim totalDamage As Double = Adamage + qDamage + rDamage
				Select Case ((enemy.Health - totalDamage) / enemy.MaxHealth)
					Case Is >= 0.66
						enemyColor(enemy.NetworkId) = System.Drawing.Color.Green
					Case 0.329 To 0.66
						enemyColor(enemy.NetworkId) = System.Drawing.Color.Yellow
					Case Is <= 0.329
						enemyColor(enemy.NetworkId) = System.Drawing.Color.Red
					Case Else
				End Select

			End If
		Next
	End Sub
#End Region
End Class
