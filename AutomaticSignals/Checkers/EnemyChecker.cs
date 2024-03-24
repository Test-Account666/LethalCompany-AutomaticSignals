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
    }

    internal static void CheckForEnemies(PlayerControllerB playerControllerB) {
        var currentTime = UnixTime.GetCurrentTime();

        if (_nextEnemyMessage > currentTime)
            return;

        var warnChance = _Random.Next(0, 100);

        if (warnChance > _warnChance.Value)
            return;

        foreach (var spawnedEnemy in from spawnedEnemy in RoundManager.Instance.SpawnedEnemies
                 where !spawnedEnemy.isEnemyDead
                 where spawnedEnemy.isOutside == !playerControllerB.isInsideFactory
                 where !ShouldIgnoreEnemy(spawnedEnemy.enemyType.enemyName)
                 let distance = Vector3.Distance(playerControllerB.transform.position, spawnedEnemy.transform.position)
                 where distance <= _minimumWarnDistance.Value
                 select spawnedEnemy) {
            _nextEnemyMessage =
                currentTime + _Random.Next(_minimumMessageCoolDown.Value, _maximumMessageCoolDown.Value);

            Transmitter.SendMessage(GetEnemyName(spawnedEnemy.enemyType.enemyName));
            break;
        }
    }

    private static bool ShouldIgnoreEnemy(string enemyName) {
        return enemyName.ToLower() switch {
            "docile locust bees" => true,
            "red locust bees" => true,
            "manticoil" => true,
            _ => false
        };
    }

    private static string GetEnemyName(string enemyName) {
        var geniusChance = _Random.Next(0, 100);
        var idiotChance = _Random.Next(0, 100);
        var colorBlindChance = _Random.Next(0, 100);

        return IsIdiotOrDressGirl(geniusChance, enemyName)
            ? GetIdiotName(idiotChance, colorBlindChance)
            : GetGeniusEnemyName(enemyName);
    }

    private static bool IsIdiotOrDressGirl(int geniusChance, string enemyName) =>
        geniusChance >= 15 || enemyName.ToLower().Equals("girl");

    private static string GetIdiotName(int idiotChance, int colorBlindChance) =>
        idiotChance <= 30 && colorBlindChance <= 3 ? "Blue Dot" :
        idiotChance <= 30 ? "Red Dot" : "Enemy";

    private static string GetGeniusEnemyName(string enemyName) => enemyName.ToLower() switch {
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
        _ => enemyName
    };

    private static string GetFlowermanName() => _Random.Next(0, 100) > 7 ? "Bracken" :
        _Random.Next(0, 100) >= 50 ? "Behind You" : "Behind U";

    private static string GetMouthDogName() => _Random.Next(0, 100) > 5 ? "Dog" : "Doggo";

    private static string GetSpiderName() => _Random.Next(0, 100) > 5 ? "Spider" : "Spooder";

    private static string GetMaskedName() => _Random.Next(0, 100) > 45 ? "Masked" : "Mimic";

    private static string GetLootBugName() => _Random.Next(0, 100) > 4 ? "Loot Bug" : "Yippee Bug";
}