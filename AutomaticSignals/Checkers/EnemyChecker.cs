using System.Linq;
using GameNetcodeStuff;
using UnityEngine;
using Random = System.Random;

namespace AutomaticSignals.Checkers;

public static class EnemyChecker {
    private const int MINIMUM_MESSAGE_COOL_DOWN = 15000;
    private const int MAXIMUM_MESSAGE_COOL_DOWN = 23000;
    private const int MINIMUM_WARN_DISTANCE = 20;
    private const int WARN_CHANCE = 45;
    private static long _nextEnemyMessage;
    private static readonly Random _Random = new();

    internal static void CheckForEnemies(PlayerControllerB playerControllerB) {
        var currentTime = UnixTime.GetCurrentTime();

        if (_nextEnemyMessage > currentTime)
            return;

        var warnChance = _Random.Next(0, 100);

        if (warnChance > WARN_CHANCE)
            return;

        foreach (var spawnedEnemy in from spawnedEnemy in RoundManager.Instance.SpawnedEnemies
                 where !spawnedEnemy.isEnemyDead
                 where spawnedEnemy.isOutside == !playerControllerB.isInsideFactory
                 where !ShouldIgnoreEnemy(spawnedEnemy.enemyType.enemyName)
                 let distance = Vector3.Distance(playerControllerB.transform.position, spawnedEnemy.transform.position)
                 where distance <= MINIMUM_WARN_DISTANCE
                 select spawnedEnemy) {
            _nextEnemyMessage = currentTime + _Random.Next(MINIMUM_MESSAGE_COOL_DOWN, MAXIMUM_MESSAGE_COOL_DOWN);

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