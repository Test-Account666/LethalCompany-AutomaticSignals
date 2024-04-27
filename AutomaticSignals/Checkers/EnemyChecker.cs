using System.Linq;
using BepInEx.Configuration;
using GameNetcodeStuff;
using UnityEngine;
using Random = System.Random;

namespace AutomaticSignals.Checkers;

[InitializeConfig]
public static class EnemyChecker {
    private static ConfigEntry<int> _minimumMessageCoolDown = null!; // Default: 15000;
    private static ConfigEntry<int> _maximumMessageCoolDown = null!; // Default: 23000;
    private static ConfigEntry<int> _minimumWarnDistance = null!; // Default: 20;
    private static ConfigEntry<int> _warnChance = null!; // Default: 45;
    private static ConfigEntry<int> _nameEnemyChance = null!; // Default: 15;
    private static ConfigEntry<int> _idiotChance = null!; // Default: 30;
    private static ConfigEntry<int> _colorblindChance = null!; // Default: 3;
    private static long _nextEnemyMessage;
    private static readonly Random _Random = new();

    public static void Initialize(ConfigFile configFile) {
        _minimumMessageCoolDown = configFile.Bind("Enemy Warning", "1. Minimum message cooldown", 15000,
                                                  "Defines the minimum cooldown (in milliseconds) to wait before warning about enemies");

        _maximumMessageCoolDown = configFile.Bind("Enemy Warning", "2. Maximum message cooldown", 23000,
                                                  "Defines the maximum cooldown (in milliseconds) to wait before warning about enemies");

        _minimumWarnDistance = configFile.Bind("Enemy Warning", "3. Minimum warn distance", 20,
                                               "Defines the minimum distance the player needs to be to an enemy for a warning to be sent");

        _warnChance = configFile.Bind("Enemy Warning", "4. Warn chance", 20,
                                      new ConfigDescription("Defines the chance for a player to be warned",
                                                            new AcceptableValueRange<int>(0, 100)));

        _nameEnemyChance = configFile.Bind("Enemy Warning", "5. Name enemy chance", 15,
                                           new ConfigDescription("Defines the chance for sending the enemy type",
                                                                 new AcceptableValueRange<int>(0, 100)));

        _idiotChance = configFile.Bind("Enemy Warning", "6. Idiot chance", 15,
                                       new ConfigDescription("Defines the chance for saying \"Red Dot\" instead of \"Enemy\"",
                                                             new AcceptableValueRange<int>(0, 100)));

        _colorblindChance = configFile.Bind("Enemy Warning", "7. Colorblind chance", 3,
                                            new ConfigDescription("Defines the chance for saying \"Blue Dot\" instead of \"Red Dot\"",
                                                                  new AcceptableValueRange<int>(0, 100)));
    }

    internal static void CheckForEnemies(PlayerControllerB playerControllerB) {
        var currentTime = UnixTime.GetCurrentTime();

        if (_nextEnemyMessage > currentTime)
            return;

        var warnChance = _Random.Next(1, 101);

        if (warnChance > _warnChance.Value)
            return;

        foreach (var spawnedEnemy in from spawnedEnemy in Object.FindObjectsOfType<EnemyAI>()
                                     where !spawnedEnemy.isEnemyDead
                                     where spawnedEnemy.isOutside == !playerControllerB.isInsideFactory
                                     where !ShouldIgnoreEnemy(spawnedEnemy.enemyType.enemyName)
                                     let distance = Vector3.Distance(playerControllerB.transform.position,
                                                                     spawnedEnemy.transform.position)
                                     where distance <= _minimumWarnDistance.Value
                                     select spawnedEnemy) {
            _nextEnemyMessage =
                currentTime + _Random.Next(_minimumMessageCoolDown.Value, _maximumMessageCoolDown.Value);

            Transmitter.SendMessage(GetEnemyName(spawnedEnemy.enemyType.enemyName));
            break;
        }
    }

    private static bool ShouldIgnoreEnemy(string enemyName) =>
        enemyName.ToLower() switch {
            "docile locust bees" => true,
            "red locust bees" => true,
            "manticoil" => true,
            var _ => false,
        };

    private static string GetEnemyName(string enemyName) {
        var geniusChance = _Random.Next(1, 101);
        var idiotChance = _Random.Next(1, 101);
        var colorBlindChance = _Random.Next(1, 101);

        return IsIdiotOrDressGirl(geniusChance, enemyName)
            ? GetIdiotName(idiotChance, colorBlindChance)
            : GetGeniusEnemyName(enemyName);
    }

    private static bool IsIdiotOrDressGirl(int geniusChance, string enemyName) =>
        geniusChance >= _nameEnemyChance.Value || enemyName.ToLower().Equals("girl");

    private static string GetIdiotName(int idiotChance, int colorBlindChance) {
        if (idiotChance <= _idiotChance.Value && colorBlindChance <= _colorblindChance.Value)
            return "Blue Dot";

        return idiotChance <= _idiotChance.Value? "Red Dot" : "Enemy";
    }

    private static string GetGeniusEnemyName(string enemyName) =>
        enemyName.ToLower() switch {
            "radmech" => "Old Bird",
            "flowerman" => GetFlowermanName(),
            "centipede" => "Snare Flea",
            "earth leviathan" => "Worm",
            "mouthdog" => GetMouthDogName(),
            "forestgiant" => "Giant",
            "crawler" => "Thumper",
            "bunker spider" => GetSpiderName(),
            "masked" => GetMaskedName(),
            "spring" => "Coil Head",
            "puffer" => "Lizard",
            "baboon hawk" => "Bird",
            "hoarding bug" => GetLootBugName(),
            var _ => enemyName,
        };

    private static string GetFlowermanName() =>
        _Random.Next(1, 101) > 7  ? "Bracken" :
        _Random.Next(1, 101) >= 50? "Behind You" : "Behind U";

    private static string GetMouthDogName() =>
        _Random.Next(1, 101) > 5? "Dog" : "Doggo";

    private static string GetSpiderName() =>
        _Random.Next(1, 101) > 5? "Spider" : "Spooder";

    private static string GetMaskedName() =>
        _Random.Next(1, 101) > 45? "Masked" : "Mimic";

    private static string GetLootBugName() =>
        _Random.Next(1, 101) > 4? "Loot Bug" : "Yippee Bug";
}