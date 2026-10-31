using System;
using UnityEngine;

public enum Language
{
    VI,
    EN
}

public static class LocalizationManager
{
    public static event Action OnLanguageChanged;

    private const string PREF_KEY = "Game_Language_Setting";
    private static Language currentLanguage = Language.VI;
    private static bool initialized = false;

    public static Language CurrentLanguage
    {
        get
        {
            if (!initialized) Init();
            return currentLanguage;
        }
        set
        {
            if (currentLanguage != value || !initialized)
            {
                currentLanguage = value;
                initialized = true;
                PlayerPrefs.SetInt(PREF_KEY, (int)currentLanguage);
                PlayerPrefs.Save();
                OnLanguageChanged?.Invoke();
            }
        }
    }

    public static bool IsVietnamese => CurrentLanguage == Language.VI;

    private static void Init()
    {
        initialized = true;
        // Default to Vietnamese (0)
        currentLanguage = (Language)PlayerPrefs.GetInt(PREF_KEY, (int)Language.VI);
    }

    public static void ToggleLanguage()
    {
        CurrentLanguage = (CurrentLanguage == Language.VI) ? Language.EN : Language.VI;
    }

    // Main Menu texts
    public static string PlayButton => IsVietnamese ? "CHƠI NGAY" : "PLAY";
    public static string GuideButton => IsVietnamese ? "HƯỚNG DẪN" : "GUIDE";
    public static string QuitButton => IsVietnamese ? "THOÁT GAME" : "QUIT";
    public static string LanguageButton => IsVietnamese ? "🇻🇳 TIẾNG VIỆT" : "en ENGLISH";
    public static string FooterText => IsVietnamese ? "BẢO VỆ VƯƠNG QUỐC  /  XÂY DỰNG ĐẾ CHẾ" : "DEFEND THE REALM  /  BUILD YOUR KINGDOM";

    // Guide Panel
    public static string GuideTitle => IsVietnamese ? "HƯỚNG DẪN CHIẾN THUẬT" : "TACTICAL GUIDE";
    public static string GuideClose => IsVietnamese ? "ĐÃ HIỂU" : "UNDERSTOOD";
    public static string GuideContent => IsVietnamese
        ? "<b>1. MỤC TIÊU CỐT LÕI</b>\n" +
          "• Bảo vệ Lâu đài sống sót qua <b>6 đợt quái vật</b> xâm lăng.\n" +
          "• Khởi đầu: <b>200 Vàng</b> & <b>20 Máu thành</b> (quái lọt vào sẽ trừ máu thành).\n\n" +
          "<b>2. HỆ THỐNG THÁP PHÒNG THỦ</b>\n" +
          "• <b>Tháp Cung (50G):</b> Tốc độ bắn cực nhanh (0.5s), tầm xa (3.5), ưu tiên quái đi đầu.\n" +
          "• <b>Tháp Pháo (100G):</b> Sát thương diện rộng (AOE), nổ lan nhiều quái, 15% bạo kích x1.5 damage.\n\n" +
          "<b>3. QUÁI VẬT XÂM LƯỢC</b>\n" +
          "• <b>Goblin:</b> Di chuyển nhanh, 50 Máu, hạ gục nhận <b>5 Vàng</b> (lọt thành trừ 1 Máu).\n" +
          "• <b>Orc:</b> Trâu bò, 150 Máu, hạ gục nhận <b>10 Vàng</b> (xuất hiện từ Đợt 4, lọt thành trừ 2 Máu).\n\n" +
          "<b>4. ĐIỀU KHIỂN & PHÍM TẮT</b>\n" +
          "• <b>Chuột trái:</b> Nhấp ô đất trống để Xây tháp. Nhấp vào tháp đã xây để <b>Nâng cấp</b> hoặc <b>Bán</b>.\n" +
          "• <b>Phím Space:</b> Bắt đầu đợt mới hoặc Bỏ qua thời gian đếm ngược giữa các wave.\n" +
          "• <b>Phím Esc / P:</b> Tạm dừng trận đấu."
        : "<b>1. CORE OBJECTIVE</b>\n" +
          "• Protect the Castle through <b>6 invading monster waves</b>.\n" +
          "• Starting resources: <b>200 Gold</b> & <b>20 Castle HP</b> (leaked monsters deplete HP).\n\n" +
          "<b>2. DEFENSE TOWERS</b>\n" +
          "• <b>Archer Tower (50G):</b> High attack rate (0.5s), long range (3.5), targets first enemy.\n" +
          "• <b>Cannon Tower (100G):</b> Area-of-effect (AOE) splash damage, 15% crit chance (x1.5 damage).\n\n" +
          "<b>3. INVADING MONSTERS</b>\n" +
          "• <b>Goblin:</b> Fast movement, 50 HP, drops <b>5 Gold</b> (breaches castle for -1 HP).\n" +
          "• <b>Orc:</b> High durability, 150 HP, drops <b>10 Gold</b> (appears from Wave 4, breaches castle for -2 HP).\n\n" +
          "<b>4. CONTROLS & SHORTCUTS</b>\n" +
          "• <b>Left Click:</b> Click empty build tile to build. Click existing tower to <b>Upgrade</b> or <b>Sell</b>.\n" +
          "• <b>Space Key:</b> Start wave immediately or skip break countdown.\n" +
          "• <b>Esc / P Key:</b> Pause the battle.";

    // HUD & In-game
    public static string WaveTitle(int cur, int total) => IsVietnamese ? $"ĐỢT {cur} / {total}" : $"WAVE {cur} / {total}";
    public static string PrepPhase => IsVietnamese ? "Giai đoạn chuẩn bị" : "Preparation Phase";
    public static string StartWaveBtn(int wave) => IsVietnamese ? $"▶ BẮT ĐẦU ĐỢT {wave} (Space)" : $"▶ START WAVE {wave} (Space)";
    public static string NeedTowerFirst => IsVietnamese ? "⚠️ CẦN ĐẶT THÁP TRƯỚC" : "⚠️ BUILD TOWER FIRST";
    public static string TipPlaceTower => IsVietnamese ? "💡 Hãy đặt tháp phòng thủ trước khi bấm Bắt đầu!" : "💡 Place defense towers before starting!";
    public static string TipMustPlaceTower => IsVietnamese ? "💡 Hãy nhấp vào ô đất trống để đặt ít nhất 1 tháp trước khi bắt đầu!" : "💡 Click an empty build tile to place at least 1 tower before starting!";
    public static string WaveBreak(int sec) => IsVietnamese ? $"Nghỉ giữa đợt: {sec}s" : $"Next wave in: {sec}s";
    public static string EarlyStartBtn(int sec) => IsVietnamese ? $"▶ BẮT ĐẦU SỚM ({sec}s) (Space)" : $"▶ START EARLY ({sec}s) (Space)";
    public static string TipSkipBreak(int sec) => IsVietnamese ? $"Bấm Space để bỏ qua {sec}s nghỉ và đánh tiếp!" : $"Press Space to skip {sec}s wait and start wave!";
    public static string EnemiesLeft(int count) => IsVietnamese ? $"Quái còn lại: {count}" : $"Enemies remaining: {count}";
    public static string BattleAction(int count) => IsVietnamese ? $"⚔ ĐANG CHIẾN ĐẤU ({count}) ⚔" : $"⚔ IN BATTLE ({count}) ⚔";

    // Pause Panel
    public static string PauseTitle => IsVietnamese ? "TẠM DỪNG" : "PAUSED";
    public static string ResumeBtn => IsVietnamese ? "TIẾP TỤC" : "RESUME";
    public static string ReplayBtn => IsVietnamese ? "CHƠI LẠI" : "REPLAY";
    public static string MenuBtn => IsVietnamese ? "VỀ MENU" : "MAIN MENU";

    // Result Panels
    public static string WinTitle => IsVietnamese ? "CHIẾN THẮNG!" : "VICTORY!";
    public static string WinSubtitle => IsVietnamese ? "VƯƠNG QUỐC ĐÃ ĐƯỢC BẢO VỆ" : "THE REALM HAS BEEN SAVED";
    public static string LoseTitle => IsVietnamese ? "THẤT BẠI!" : "DEFEAT!";
    public static string LoseSubtitle => IsVietnamese ? "LÂU ĐÀI ĐÃ THẤT THỦ" : "THE CASTLE HAS FALLEN";

    public static string WavesCompleted => IsVietnamese ? "SỐ ĐỢT VƯỢT QUA" : "WAVES COMPLETED";
    public static string WaveReached => IsVietnamese ? "ĐỢT SỐNG SÓT" : "WAVE REACHED";
    public static string EnemiesDefeated => IsVietnamese ? "QUÁI ĐÃ TIÊU DIỆT" : "ENEMIES DEFEATED";
    public static string BaseHp => IsVietnamese ? "MÁU THÀNH" : "BASE HP";
    public static string GoldEarned => IsVietnamese ? "VÀNG THU THẬP" : "GOLD EARNED";
    public static string Score => IsVietnamese ? "ĐIỂM SỐ" : "SCORE";
    public static string NextLevel => IsVietnamese ? "MÀN TIẾP THEO" : "NEXT LEVEL";

    // Build Manager
    public static string Upgrade => IsVietnamese ? "Nâng cấp" : "Upgrade";
    public static string Sell => IsVietnamese ? "Bán tháp" : "Sell";
    public static string ClickAgain => IsVietnamese ? "nhấp lại để xác nhận" : "click again to confirm";

        // ===== TÊN HIỂN THỊ CỦA THÁP & QUÁI: muốn đổi tên thì sửa chữ trong dấu "" ở đây =====
    public static string GetTowerDisplayName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return "";
        if (rawName.IndexOf("Archer", StringComparison.OrdinalIgnoreCase) >= 0)
            return IsVietnamese ? "Tháp Cung" : "Archer";
        if (rawName.IndexOf("Cannon", StringComparison.OrdinalIgnoreCase) >= 0)
            return IsVietnamese ? "Tháp Pháo" : "Cannon";
        if (rawName.IndexOf("Magic", StringComparison.OrdinalIgnoreCase) >= 0)
            return IsVietnamese ? "Tháp Phép" : "Magic";
        return rawName;
    }

    public static string GetEnemyDisplayName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return "";
        if (rawName.IndexOf("Goblin", StringComparison.OrdinalIgnoreCase) >= 0)
            return IsVietnamese ? "Yêu Tinh" : "Goblin";
        if (rawName.IndexOf("Orc", StringComparison.OrdinalIgnoreCase) >= 0)
            return IsVietnamese ? "Thú Nhân" : "Orc";
        return rawName;
    }
}
