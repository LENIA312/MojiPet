using UnityEngine;

namespace Mojipet.UI.Components
{
    /// <summary>
    /// 「もじぺっと」全画面共通の配色定義。個々のViewが色を直接指定せず、ここを経由することで
    /// 見た目の一貫性を保つ。暖色寄りの落ち着いた配色(言霊・和紙をイメージした紫〜珊瑚色)。
    /// </summary>
    public static class UiTheme
    {
        // ウィンドウ背景(モーダルの地色)
        public static readonly Color WindowBackground = new Color(0.169f, 0.141f, 0.220f, 0.94f);

        // ヘッダー・Toastなど、ウィンドウ背景よりわずかに明るい面
        public static readonly Color Surface = new Color(0.290f, 0.247f, 0.361f, 0.92f);

        // 入力欄・リスト背景など、さらに一段明るい面
        public static readonly Color SurfaceLight = new Color(1f, 1f, 1f, 0.08f);

        // メインアクション(購入・強化・研究する・決定・使う 等)
        public static readonly Color Primary = new Color(1.000f, 0.541f, 0.396f, 1f);

        // 副次アクション(閉じる・ページ送り 等)
        public static readonly Color Secondary = new Color(0.545f, 0.498f, 0.659f, 1f);

        // 無効化されたボタン
        public static readonly Color Disabled = new Color(0.361f, 0.329f, 0.408f, 1f);

        // 警告・不足・失敗メッセージ
        public static readonly Color Danger = new Color(0.898f, 0.451f, 0.451f, 1f);

        // 成功・肯定的な状態
        public static readonly Color Success = new Color(0.506f, 0.788f, 0.584f, 1f);

        // 本文テキスト
        public static readonly Color TextPrimary = new Color(0.961f, 0.941f, 1.000f, 1f);

        // 補足テキスト
        public static readonly Color TextMuted = new Color(0.722f, 0.686f, 0.788f, 1f);

        // ボタン上のテキスト
        public static readonly Color TextOnPrimary = Color.white;
    }
}
