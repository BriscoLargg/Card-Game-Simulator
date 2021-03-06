﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CGS.Menu
{
    public class TitleScreen : MonoBehaviour
    {
        public const float MinWidth = 1080;
        public const int CenterTextFontSizePortrait = 30;
        public const int CenterTextFontSizeLandscape = 40;
        public const int MetaTextFontSizePortrait = 25;
        public const int MetaTextFontSizeLandscape = 30;

        public static readonly Vector2 CompanyTextPortraitDimensions = new Vector2(250, 75);
        public static readonly Vector2 CompanyTextLandscapeDimensions = new Vector2(-400, 75);
        public static readonly Vector2 CompanyTextLandscapePosition = new Vector2(-700, 0);
        public static readonly Vector2 VersionTextPortraitOffsetMin = new Vector2(-250, 0);
        public static readonly Vector2 VersionTextPortraitOffsetMax = new Vector2(0, 75);
        public static readonly Vector2 VersionTextLandscapeOffsetMin = new Vector2(400, 0);
        public static readonly Vector2 VersionTextLandscapeOffsetMax = new Vector2(700, 75);

        public Image backgroundImage;
        public Sprite backgroundSpritePortrait;
        public Sprite backgroundSpriteLandscape;
        public Image footerImage;
        public Sprite footerSpritePortrait;
        public Sprite footerSpriteLandscape;
        public Text companyText;
        public Text centerText;
        public Text versionText;

        void OnRectTransformDimensionsChange()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (((RectTransform)transform).rect.width < MinWidth) // Portrait
            {
                backgroundImage.sprite = backgroundSpritePortrait;
                footerImage.preserveAspect = false;
                footerImage.sprite = footerSpritePortrait;
                centerText.fontSize = CenterTextFontSizePortrait;
                companyText.fontSize = MetaTextFontSizePortrait;
                companyText.rectTransform.anchorMin = Vector2.zero;
                companyText.rectTransform.anchorMax = Vector2.zero;
                companyText.rectTransform.pivot = Vector2.zero;
                companyText.rectTransform.offsetMin = Vector2.zero;
                companyText.rectTransform.offsetMax = CompanyTextPortraitDimensions;
                versionText.fontSize = MetaTextFontSizePortrait;
                versionText.rectTransform.anchorMin = Vector2.right;
                versionText.rectTransform.anchorMax = Vector2.right;
                versionText.rectTransform.pivot = Vector2.right;
                versionText.rectTransform.offsetMin = VersionTextPortraitOffsetMin;
                versionText.rectTransform.offsetMax = VersionTextPortraitOffsetMax;
            }
            else // Landscape
            {
                backgroundImage.sprite = backgroundSpriteLandscape;
                footerImage.preserveAspect = true;
                footerImage.sprite = footerSpriteLandscape;
                centerText.fontSize = CenterTextFontSizeLandscape;
                companyText.fontSize = MetaTextFontSizeLandscape;
                companyText.rectTransform.anchorMin = Vector2.right / 2;
                companyText.rectTransform.anchorMax = Vector2.right / 2;
                companyText.rectTransform.pivot = Vector2.right / 2;
                companyText.rectTransform.offsetMin = CompanyTextLandscapePosition;
                companyText.rectTransform.offsetMax = CompanyTextLandscapeDimensions;
                versionText.fontSize = MetaTextFontSizeLandscape;
                versionText.rectTransform.anchorMin = Vector2.right / 2;
                versionText.rectTransform.anchorMax = Vector2.right / 2;
                versionText.rectTransform.pivot = Vector2.right / 2;
                versionText.rectTransform.offsetMin = VersionTextLandscapeOffsetMin;
                versionText.rectTransform.offsetMax = VersionTextLandscapeOffsetMax;
            }
        }

        void Start()
        {
            versionText.text = MainMenu.VersionMessage;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Branch.initSession(CardGameManager.Instance.BranchCallbackWithParams);
#endif
        }

        void Update()
        {
            if (CardGameManager.Instance.ModalCanvas != null)
                return;

            if (Input.anyKeyDown)
                SceneManager.LoadScene(MainMenu.MainMenuSceneIndex);
        }
    }
}
