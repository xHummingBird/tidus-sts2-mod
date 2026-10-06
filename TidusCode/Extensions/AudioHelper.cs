using MegaCrit.Sts2.Core.Commands;

namespace Tidus.TidusCode.Extensions;

public static class AudioHelper
{
    private static readonly Random rng = new Random();
    
    private static readonly string[] attackSfx =
    {
        "res://Tidus/sounds/attack (1).wav",
        "res://Tidus/sounds/attack (2).wav",
        "res://Tidus/sounds/attack (3).wav",
        "res://Tidus/sounds/attack (4).wav",
        "res://Tidus/sounds/attack (5).wav",
        "res://Tidus/sounds/attack (6).wav",
    };
    
    private static readonly string[] damagedSfx =
    {
        "res://Tidus/sounds/hit (1).wav",
        "res://Tidus/sounds/hit (2).wav",
        "res://Tidus/sounds/hit (3).wav",
        
    };
    
    private static readonly string[] highDamagedSfx =
    {
        "res://Tidus/sounds/hit_high (1).wav",
        "res://Tidus/sounds/hit_high (2).wav",
        "res://Tidus/sounds/hit_high (3).wav",
    };
    
    private static readonly string[] criticalDamagedSfx =
    {
        "res://Tidus/sounds/hit_critical (1).wav",
        "res://Tidus/sounds/hit_critical (2).wav",
        "res://Tidus/sounds/hit_critical (3).wav",
    };
    
    private static readonly string[] defendSfx =
    {
        "res://Tidus/sounds/dodge (1).wav",
        "res://Tidus/sounds/dodge (2).wav",
        "res://Tidus/sounds/dodge (3).wav",
        "res://Tidus/sounds/dodge (4).wav",
    };
    
    private static readonly string[] victorySfx =
    {
        "res://Tidus/sounds/victory_1.wav",
        "res://Tidus/sounds/victory_2.wav",
        "res://Tidus/sounds/victory_3.wav",
        "res://Tidus/sounds/victory_4.wav",
        "res://Tidus/sounds/victory_5.wav",
        "res://Tidus/sounds/victory_6.wav",
    };

    private static readonly string[] gameoverSfx =
    {
        "res://Tidus/sounds/gameover (1).wav",
        "res://Tidus/sounds/gameover (2).wav",
        "res://Tidus/sounds/gameover (3).wav",
        "res://Tidus/sounds/gameover (4).wav",
        "res://Tidus/sounds/gameover (5).wav",
    };
    
    private static readonly string[] phraseSfx =
    {
        "res://Tidus/sounds/phrase (1).wav",
        "res://Tidus/sounds/phrase (2).wav",
        "res://Tidus/sounds/phrase (3).wav",
        "res://Tidus/sounds/phrase (4).wav",
        "res://Tidus/sounds/phrase (5).wav",
        "res://Tidus/sounds/phrase (6).wav",
        "res://Tidus/sounds/phrase (7).wav",
        "res://Tidus/sounds/phrase (8).wav",
        "res://Tidus/sounds/phrase (9).wav",
    };

    private static readonly string[] attackHighSfx =
    {
        "res://Tidus/sounds/attack_hard (1).wav",
        "res://Tidus/sounds/attack_hard (2).wav",
        "res://Tidus/sounds/attack_hard (3).wav",
        "res://Tidus/sounds/attack_hard (4).wav",
        "res://Tidus/sounds/attack_hard (6).wav",
        "res://Tidus/sounds/attack_hard (7).wav",
        "res://Tidus/sounds/attack_hard (8).wav",
        "res://Tidus/sounds/attack_hard (9).wav",
    };
    
    private static readonly string[] lastHitSfx =
    {
        "res://Tidus/sounds/last_hit (1).wav",
        "res://Tidus/sounds/last_hit (2).wav",
        "res://Tidus/sounds/last_hit (3).wav",
        "res://Tidus/sounds/last_hit (4).wav",
        "res://Tidus/sounds/last_hit (5).wav",
        "res://Tidus/sounds/last_hit (6).wav",
        "res://Tidus/sounds/last_hit (7).wav",
        "res://Tidus/sounds/last_hit (8).wav",
        "res://Tidus/sounds/last_hit (9).wav",
        "res://Tidus/sounds/last_hit (10).wav",
        "res://Tidus/sounds/last_hit (11).wav",
        "res://Tidus/sounds/last_hit (12).wav",
        "res://Tidus/sounds/last_hit (13).wav",
        "res://Tidus/sounds/last_hit (14).wav",
        "res://Tidus/sounds/last_hit (15).wav",
        "res://Tidus/sounds/last_hit (16).wav",
        "res://Tidus/sounds/last_hit (17).wav",
        "res://Tidus/sounds/last_hit (18).wav",
        "res://Tidus/sounds/last_hit (19).wav",
    };
    
    private static readonly string[] limitBreakSfx =
    {
        "res://Tidus/sounds/special (1).wav",
        "res://Tidus/sounds/special (2).wav",
    };
    
    public static void PlayRandomAttack()
    {
        PlayRandom(attackSfx);
    }
    
    public static void PlayRandomDefend()
    {
        PlayRandom(defendSfx);
    }

    public static void PlayRandomPhrase()
    {
        PlayRandom(phraseSfx);
    }
    
    public static void PlayRandomDamaged()
    {
        PlayRandom(damagedSfx);
    }

    public static void PlayRandomDamagedHigh()
    {
        PlayRandom(highDamagedSfx);
    }

    public static void PlayRandomGameover()
    {
        PlayRandom(gameoverSfx);
    }

    public static void PlayRandomDamagedCritical()
    {
        PlayRandom(criticalDamagedSfx);
    }
    
    public static void PlayRandomVictory()
    {
        PlayRandom(victorySfx);
    }

    public static void PlayRandomAttackHard()
    {
        PlayRandom(attackHighSfx);
    }

    public static void PlayRandomLastHit()
    {
        PlayRandom(lastHitSfx);
    }

    public static void PlayRandomLimitBreak()
    {
        PlayRandom(limitBreakSfx);
    }

    public static void PlayRandom(string[] pool)
    {
        int index = rng.Next(pool.Length);
        SfxCmd.Play(pool[index]);
    }
}