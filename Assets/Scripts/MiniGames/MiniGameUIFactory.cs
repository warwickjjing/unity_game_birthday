using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 미니게임 UI를 런타임에 생성하는 팩토리 클래스입니다.
    /// Unity Editor에서 수동 설정 없이 코드로 모든 UI를 생성합니다.
    /// </summary>
    public static class MiniGameUIFactory
    {
        /// <summary>
        /// 미니게임용 Canvas를 생성합니다.
        /// </summary>
        /// <returns>생성된 Canvas</returns>
        public static Canvas CreateMiniGameCanvas()
        {
            // Canvas GameObject 생성
            GameObject canvasObj = new GameObject("MiniGameCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 다른 UI 위에 표시

            // Canvas Scaler 추가
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Graphic Raycaster 추가
            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("[MiniGameUIFactory] Canvas 생성 완료");
            return canvas;
        }

        /// <summary>
        /// 설탕 미니게임 Panel과 모든 하위 UI 요소를 생성합니다.
        /// </summary>
        /// <param name="parentCanvas">부모 Canvas</param>
        /// <returns>생성된 Panel GameObject</returns>
        public static GameObject CreateSugarMiniGamePanel(Canvas parentCanvas)
        {
            if (parentCanvas == null)
            {
                Debug.LogError("[MiniGameUIFactory] Canvas가 null입니다!");
                return null;
            }

            // Panel 생성
            GameObject panelObj = new GameObject("SugarMiniGamePanel");
            panelObj.transform.SetParent(parentCanvas.transform, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f); // 반투명 검정 배경
            // 기본 Sprite 할당 (보이도록)
            SetDefaultSprite(panelImage);

            // Title Text 생성
            GameObject titleObj = CreateTextMeshPro("TitleText", panelObj.transform);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 200);
            titleRect.sizeDelta = new Vector2(800, 100);
            TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "설탕 계량하기";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;

            // Instructions Text 생성
            GameObject instructionsObj = CreateTextMeshPro("InstructionsText", panelObj.transform);
            RectTransform instructionsRect = instructionsObj.GetComponent<RectTransform>();
            instructionsRect.anchoredPosition = new Vector2(0, 150);
            instructionsRect.sizeDelta = new Vector2(800, 50);
            TextMeshProUGUI instructionsText = instructionsObj.GetComponent<TextMeshProUGUI>();
            instructionsText.text = "마우스를 눌러 설탕을 따르세요!";
            instructionsText.fontSize = 28;
            instructionsText.alignment = TextAlignmentOptions.Center;

            // Timer Text 생성
            GameObject timerObj = CreateTextMeshPro("TimerText", panelObj.transform);
            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchoredPosition = new Vector2(0, -200);
            timerRect.sizeDelta = new Vector2(400, 50);
            TextMeshProUGUI timerText = timerObj.GetComponent<TextMeshProUGUI>();
            timerText.text = "남은 시간: 10.0초";
            timerText.fontSize = 32;
            timerText.alignment = TextAlignmentOptions.Center;

            // Container (게이지 배경) 생성
            GameObject containerObj = new GameObject("Container");
            containerObj.transform.SetParent(panelObj.transform, false);
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchoredPosition = Vector2.zero; // 화면 중앙
            containerRect.sizeDelta = new Vector2(200, 400);
            Image containerImage = containerObj.AddComponent<Image>();
            containerImage.color = new Color(0.7f, 0.7f, 0.7f, 1f); // 회색
            SetDefaultSprite(containerImage);

            // SugarFill (채워지는 설탕) 생성
            GameObject sugarFillObj = new GameObject("SugarFill");
            sugarFillObj.transform.SetParent(containerObj.transform, false);
            RectTransform sugarFillRect = sugarFillObj.AddComponent<RectTransform>();
            sugarFillRect.anchorMin = Vector2.zero;
            sugarFillRect.anchorMax = Vector2.one;
            sugarFillRect.sizeDelta = Vector2.zero;
            sugarFillRect.anchoredPosition = Vector2.zero;
            Image sugarFillImage = sugarFillObj.AddComponent<Image>();
            sugarFillImage.color = Color.yellow; // 노란색으로 변경 (더 잘 보이도록)
            sugarFillImage.type = Image.Type.Filled;
            sugarFillImage.fillMethod = Image.FillMethod.Vertical;
            sugarFillImage.fillOrigin = 0; // Bottom
            sugarFillImage.fillAmount = 0f;
            SetDefaultSprite(sugarFillImage);

            // TargetZone (목표 영역) 생성 - Container의 자식으로 변경
            GameObject targetZoneObj = new GameObject("TargetZone");
            targetZoneObj.transform.SetParent(containerObj.transform, false);
            RectTransform targetZoneRect = targetZoneObj.AddComponent<RectTransform>();
            // Container 높이 400의 상단 80-100% 영역 표시 (목표 범위)
            // Container 기준으로 상단 20% 영역 (80-100%)
            targetZoneRect.anchorMin = new Vector2(-0.05f, 0.8f); // Container보다 약간 넓게, 상단 80% 위치
            targetZoneRect.anchorMax = new Vector2(1.05f, 1.0f); // 상단 100%
            targetZoneRect.sizeDelta = Vector2.zero;
            targetZoneRect.anchoredPosition = Vector2.zero;
            Image targetZoneImage = targetZoneObj.AddComponent<Image>();
            targetZoneImage.color = new Color(0, 1, 0, 0.2f); // 초록색 반투명 (더 투명하게)
            SetDefaultSprite(targetZoneImage);

            // ResultPanel 생성
            GameObject resultPanelObj = new GameObject("ResultPanel");
            resultPanelObj.transform.SetParent(panelObj.transform, false);
            RectTransform resultPanelRect = resultPanelObj.AddComponent<RectTransform>();
            resultPanelRect.anchoredPosition = Vector2.zero;
            resultPanelRect.sizeDelta = new Vector2(600, 300);
            Image resultPanelImage = resultPanelObj.AddComponent<Image>();
            resultPanelImage.color = new Color(0, 0, 0, 0.9f); // 거의 불투명한 검정
            SetDefaultSprite(resultPanelImage);
            resultPanelObj.SetActive(false); // 초기 비활성

            // ResultText 생성
            GameObject resultTextObj = CreateTextMeshPro("ResultText", resultPanelObj.transform);
            RectTransform resultTextRect = resultTextObj.GetComponent<RectTransform>();
            resultTextRect.anchoredPosition = new Vector2(0, 50);
            resultTextRect.sizeDelta = new Vector2(500, 100);
            TextMeshProUGUI resultText = resultTextObj.GetComponent<TextMeshProUGUI>();
            resultText.text = "성공!";
            resultText.fontSize = 48;
            resultText.alignment = TextAlignmentOptions.Center;

            // RetryButton 생성
            GameObject retryButtonObj = CreateButton("RetryButton", resultPanelObj.transform);
            RectTransform retryButtonRect = retryButtonObj.GetComponent<RectTransform>();
            retryButtonRect.anchoredPosition = new Vector2(-150, -50);
            retryButtonRect.sizeDelta = new Vector2(200, 60);
            Button retryButton = retryButtonObj.GetComponent<Button>();
            TextMeshProUGUI retryButtonText = retryButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (retryButtonText != null)
            {
                retryButtonText.text = "다시 시도";
                retryButtonText.fontSize = 24;
            }

            // ContinueButton 생성
            GameObject continueButtonObj = CreateButton("ContinueButton", resultPanelObj.transform);
            RectTransform continueButtonRect = continueButtonObj.GetComponent<RectTransform>();
            continueButtonRect.anchoredPosition = new Vector2(150, -50);
            continueButtonRect.sizeDelta = new Vector2(200, 60);
            Button continueButton = continueButtonObj.GetComponent<Button>();
            TextMeshProUGUI continueButtonText = continueButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (continueButtonText != null)
            {
                continueButtonText.text = "계속하기";
                continueButtonText.fontSize = 24;
            }

            Debug.Log("[MiniGameUIFactory] SugarMiniGamePanel 생성 완료");
            return panelObj;
        }

        /// <summary>
        /// 간단한 미니게임 Panel을 생성합니다 (기본 구조만).
        /// </summary>
        /// <param name="parentCanvas">부모 Canvas</param>
        /// <param name="panelName">Panel 이름</param>
        /// <param name="title">타이틀 텍스트</param>
        /// <param name="instructions">안내 텍스트</param>
        /// <returns>생성된 Panel GameObject</returns>
        public static GameObject CreateSimpleMiniGamePanel(Canvas parentCanvas, string panelName, string title, string instructions)
        {
            if (parentCanvas == null)
            {
                Debug.LogError("[MiniGameUIFactory] Canvas가 null입니다!");
                return null;
            }

            // Panel 생성
            GameObject panelObj = new GameObject(panelName);
            panelObj.transform.SetParent(parentCanvas.transform, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            SetDefaultSprite(panelImage);

            // Title Text 생성
            GameObject titleObj = CreateTextMeshPro("TitleText", panelObj.transform);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 300); // Y 위치를 더 위로
            titleRect.sizeDelta = new Vector2(1000, 80); // 크기 조정
            TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableWordWrapping = false; // 단어 줄바꿈 비활성화

            // Instructions Text 생성
            GameObject instructionsObj = CreateTextMeshPro("InstructionsText", panelObj.transform);
            RectTransform instructionsRect = instructionsObj.GetComponent<RectTransform>();
            instructionsRect.anchoredPosition = new Vector2(0, 200); // Title과 충분한 간격
            instructionsRect.sizeDelta = new Vector2(1000, 60); // 크기 조정
            TextMeshProUGUI instructionsText = instructionsObj.GetComponent<TextMeshProUGUI>();
            instructionsText.text = instructions;
            instructionsText.fontSize = 28;
            instructionsText.alignment = TextAlignmentOptions.Center;
            instructionsText.enableWordWrapping = false; // 단어 줄바꿈 비활성화

            // Timer Text 생성
            GameObject timerObj = CreateTextMeshPro("TimerText", panelObj.transform);
            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchoredPosition = new Vector2(0, -100);
            timerRect.sizeDelta = new Vector2(400, 50);
            TextMeshProUGUI timerText = timerObj.GetComponent<TextMeshProUGUI>();
            timerText.text = "남은 시간: 10.0초";
            timerText.fontSize = 32;
            timerText.alignment = TextAlignmentOptions.Center;

            // ResultPanel 생성
            GameObject resultPanelObj = new GameObject("ResultPanel");
            resultPanelObj.transform.SetParent(panelObj.transform, false);
            RectTransform resultPanelRect = resultPanelObj.AddComponent<RectTransform>();
            resultPanelRect.anchoredPosition = Vector2.zero;
            resultPanelRect.sizeDelta = new Vector2(600, 300);
            Image resultPanelImage = resultPanelObj.AddComponent<Image>();
            resultPanelImage.color = new Color(0, 0, 0, 0.9f);
            SetDefaultSprite(resultPanelImage);
            resultPanelObj.SetActive(false);

            // ResultText 생성
            GameObject resultTextObj = CreateTextMeshPro("ResultText", resultPanelObj.transform);
            RectTransform resultTextRect = resultTextObj.GetComponent<RectTransform>();
            resultTextRect.anchoredPosition = new Vector2(0, 50);
            resultTextRect.sizeDelta = new Vector2(500, 100);
            TextMeshProUGUI resultText = resultTextObj.GetComponent<TextMeshProUGUI>();
            resultText.text = "성공!";
            resultText.fontSize = 48;
            resultText.alignment = TextAlignmentOptions.Center;

            // RetryButton 생성
            GameObject retryButtonObj = CreateButton("RetryButton", resultPanelObj.transform);
            RectTransform retryButtonRect = retryButtonObj.GetComponent<RectTransform>();
            retryButtonRect.anchoredPosition = new Vector2(-150, -50);
            retryButtonRect.sizeDelta = new Vector2(200, 60);
            Button retryButton = retryButtonObj.GetComponent<Button>();
            TextMeshProUGUI retryButtonText = retryButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (retryButtonText != null)
            {
                retryButtonText.text = "다시 시도";
                retryButtonText.fontSize = 24;
            }

            // ContinueButton 생성
            GameObject continueButtonObj = CreateButton("ContinueButton", resultPanelObj.transform);
            RectTransform continueButtonRect = continueButtonObj.GetComponent<RectTransform>();
            continueButtonRect.anchoredPosition = new Vector2(150, -50);
            continueButtonRect.sizeDelta = new Vector2(200, 60);
            Button continueButton = continueButtonObj.GetComponent<Button>();
            TextMeshProUGUI continueButtonText = continueButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (continueButtonText != null)
            {
                continueButtonText.text = "계속하기";
                continueButtonText.fontSize = 24;
            }

            // SimpleMiniGame 컴포넌트 추가 및 참조 연결
            SimpleMiniGame miniGame = panelObj.AddComponent<SimpleMiniGame>();
            var componentType = typeof(SimpleMiniGame);
            var fields = componentType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Title Text
            var titleField = System.Array.Find(fields, f => f.Name == "titleText");
            if (titleField != null) titleField.SetValue(miniGame, titleText);

            // Timer Text
            var timerField = System.Array.Find(fields, f => f.Name == "timerText");
            if (timerField != null) timerField.SetValue(miniGame, timerText);

            // Instructions Text
            var instructionsField = System.Array.Find(fields, f => f.Name == "instructionsText");
            if (instructionsField != null) instructionsField.SetValue(miniGame, instructionsText);

            // Result Panel
            var resultPanelField = System.Array.Find(fields, f => f.Name == "resultPanel");
            if (resultPanelField != null) resultPanelField.SetValue(miniGame, resultPanelObj);

            // Result Text
            var resultTextField = System.Array.Find(fields, f => f.Name == "resultText");
            if (resultTextField != null) resultTextField.SetValue(miniGame, resultText);

            // Retry Button
            var retryButtonField = System.Array.Find(fields, f => f.Name == "retryButton");
            if (retryButtonField != null) retryButtonField.SetValue(miniGame, retryButton);

            // Continue Button
            var continueButtonField = System.Array.Find(fields, f => f.Name == "continueButton");
            if (continueButtonField != null) continueButtonField.SetValue(miniGame, continueButton);

            Debug.Log($"[MiniGameUIFactory] {panelName} 생성 완료");
            return panelObj;
        }

        /// <summary>
        /// 계란 배달 미니게임 Panel을 생성합니다.
        /// </summary>
        public static GameObject CreateEggMiniGamePanel(Canvas parentCanvas)
        {
            GameObject panel = CreateSimpleMiniGamePanel(parentCanvas, "EggMiniGamePanel", "계란 배달하기", "화살표 키로 조심스럽게 이동하세요!");
            
            // EggDeliveryMiniGame 컴포넌트 추가 및 설정
            EggDeliveryMiniGame miniGame = panel.AddComponent<EggDeliveryMiniGame>();
            var componentType = typeof(EggDeliveryMiniGame);
            var fields = componentType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // UI 참조 연결
            Transform titleText = panel.transform.Find("TitleText");
            Transform timerText = panel.transform.Find("TimerText");
            Transform instructionsText = panel.transform.Find("InstructionsText");
            Transform resultPanel = panel.transform.Find("ResultPanel");
            Transform resultText = resultPanel != null ? resultPanel.Find("ResultText") : null;
            Transform retryButton = resultPanel != null ? resultPanel.Find("RetryButton") : null;
            Transform continueButton = resultPanel != null ? resultPanel.Find("ContinueButton") : null;

            var timerField = System.Array.Find(fields, f => f.Name == "timerText");
            if (timerField != null && timerText != null) timerField.SetValue(miniGame, timerText.GetComponent<TextMeshProUGUI>());

            var instructionsField = System.Array.Find(fields, f => f.Name == "instructionsText");
            if (instructionsField != null && instructionsText != null) instructionsField.SetValue(miniGame, instructionsText.GetComponent<TextMeshProUGUI>());

            var resultPanelField = System.Array.Find(fields, f => f.Name == "resultPanel");
            if (resultPanelField != null && resultPanel != null) resultPanelField.SetValue(miniGame, resultPanel.gameObject);

            var resultTextField = System.Array.Find(fields, f => f.Name == "resultText");
            if (resultTextField != null && resultText != null) resultTextField.SetValue(miniGame, resultText.GetComponent<TextMeshProUGUI>());

            var retryButtonField = System.Array.Find(fields, f => f.Name == "retryButton");
            if (retryButtonField != null && retryButton != null) retryButtonField.SetValue(miniGame, retryButton.GetComponent<Button>());

            var continueButtonField = System.Array.Find(fields, f => f.Name == "continueButton");
            if (continueButtonField != null && continueButton != null) continueButtonField.SetValue(miniGame, continueButton.GetComponent<Button>());

            // 균형 게이지 생성 (EggDeliveryMiniGame의 Awake에서 자동 생성되지만 미리 만들어도 됨)
            
            Debug.Log("[MiniGameUIFactory] EggMiniGamePanel 생성 완료");
            return panel;
        }

        /// <summary>
        /// 밀가루 쌓기 미니게임 Panel을 생성합니다.
        /// </summary>
        public static GameObject CreateFlourMiniGamePanel(Canvas parentCanvas)
        {
            GameObject panel = CreateSimpleMiniGamePanel(parentCanvas, "FlourMiniGamePanel", "밀가루 포대 쌓기", "좌우 화살표 키로 포대를 받아내세요!");
            
            // FlourStackingMiniGame 컴포넌트 추가 및 설정
            FlourStackingMiniGame miniGame = panel.AddComponent<FlourStackingMiniGame>();
            var componentType = typeof(FlourStackingMiniGame);
            var fields = componentType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // UI 참조 연결
            Transform timerText = panel.transform.Find("TimerText");
            Transform instructionsText = panel.transform.Find("InstructionsText");
            Transform resultPanel = panel.transform.Find("ResultPanel");
            Transform resultText = resultPanel != null ? resultPanel.Find("ResultText") : null;
            Transform retryButton = resultPanel != null ? resultPanel.Find("RetryButton") : null;
            Transform continueButton = resultPanel != null ? resultPanel.Find("ContinueButton") : null;

            var timerField = System.Array.Find(fields, f => f.Name == "timerText");
            if (timerField != null && timerText != null) timerField.SetValue(miniGame, timerText.GetComponent<TextMeshProUGUI>());

            var instructionsField = System.Array.Find(fields, f => f.Name == "instructionsText");
            if (instructionsField != null && instructionsText != null) instructionsField.SetValue(miniGame, instructionsText.GetComponent<TextMeshProUGUI>());

            var resultPanelField = System.Array.Find(fields, f => f.Name == "resultPanel");
            if (resultPanelField != null && resultPanel != null) resultPanelField.SetValue(miniGame, resultPanel.gameObject);

            var resultTextField = System.Array.Find(fields, f => f.Name == "resultText");
            if (resultTextField != null && resultText != null) resultTextField.SetValue(miniGame, resultText.GetComponent<TextMeshProUGUI>());

            var retryButtonField = System.Array.Find(fields, f => f.Name == "retryButton");
            if (retryButtonField != null && retryButton != null) retryButtonField.SetValue(miniGame, retryButton.GetComponent<Button>());

            var continueButtonField = System.Array.Find(fields, f => f.Name == "continueButton");
            if (continueButtonField != null && continueButton != null) continueButtonField.SetValue(miniGame, continueButton.GetComponent<Button>());

            // Count Text 추가
            GameObject countObj = CreateTextMeshPro("CountText", panel.transform);
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchoredPosition = new Vector2(0, -50);
            countRect.sizeDelta = new Vector2(400, 50);
            TextMeshProUGUI countText = countObj.GetComponent<TextMeshProUGUI>();
            countText.text = "쌓은 포대: 0 / 10";
            countText.fontSize = 32;
            countText.alignment = TextAlignmentOptions.Center;

            var countTextField = System.Array.Find(fields, f => f.Name == "countText");
            if (countTextField != null) countTextField.SetValue(miniGame, countText);

            Debug.Log("[MiniGameUIFactory] FlourMiniGamePanel 생성 완료");
            return panel;
        }

        /// <summary>
        /// 딸기 따기 미니게임 Panel을 생성합니다.
        /// </summary>
        public static GameObject CreateStrawberryMiniGamePanel(Canvas parentCanvas)
        {
            GameObject panel = CreateSimpleMiniGamePanel(parentCanvas, "StrawberryMiniGamePanel", "딸기 따기", "좋은 딸기를 클릭하세요!");
            
            // StrawberryPickingMiniGame 컴포넌트 추가 및 설정
            StrawberryPickingMiniGame miniGame = panel.AddComponent<StrawberryPickingMiniGame>();
            var componentType = typeof(StrawberryPickingMiniGame);
            var fields = componentType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // UI 참조 연결
            Transform timerText = panel.transform.Find("TimerText");
            Transform instructionsText = panel.transform.Find("InstructionsText");
            Transform resultPanel = panel.transform.Find("ResultPanel");
            Transform resultText = resultPanel != null ? resultPanel.Find("ResultText") : null;
            Transform retryButton = resultPanel != null ? resultPanel.Find("RetryButton") : null;
            Transform continueButton = resultPanel != null ? resultPanel.Find("ContinueButton") : null;

            var timerField = System.Array.Find(fields, f => f.Name == "timerText");
            if (timerField != null && timerText != null) timerField.SetValue(miniGame, timerText.GetComponent<TextMeshProUGUI>());

            var instructionsField = System.Array.Find(fields, f => f.Name == "instructionsText");
            if (instructionsField != null && instructionsText != null) instructionsField.SetValue(miniGame, instructionsText.GetComponent<TextMeshProUGUI>());

            var resultPanelField = System.Array.Find(fields, f => f.Name == "resultPanel");
            if (resultPanelField != null && resultPanel != null) resultPanelField.SetValue(miniGame, resultPanel.gameObject);

            var resultTextField = System.Array.Find(fields, f => f.Name == "resultText");
            if (resultTextField != null && resultText != null) resultTextField.SetValue(miniGame, resultText.GetComponent<TextMeshProUGUI>());

            var retryButtonField = System.Array.Find(fields, f => f.Name == "retryButton");
            if (retryButtonField != null && retryButton != null) retryButtonField.SetValue(miniGame, retryButton.GetComponent<Button>());

            var continueButtonField = System.Array.Find(fields, f => f.Name == "continueButton");
            if (continueButtonField != null && continueButton != null) continueButtonField.SetValue(miniGame, continueButton.GetComponent<Button>());

            // Count Text 추가
            GameObject countObj = CreateTextMeshPro("CountText", panel.transform);
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchoredPosition = new Vector2(0, -50);
            countRect.sizeDelta = new Vector2(400, 50);
            TextMeshProUGUI countText = countObj.GetComponent<TextMeshProUGUI>();
            countText.text = "수집: 0 / 5";
            countText.fontSize = 32;
            countText.alignment = TextAlignmentOptions.Center;

            var countTextField = System.Array.Find(fields, f => f.Name == "countText");
            if (countTextField != null) countTextField.SetValue(miniGame, countText);

            Debug.Log("[MiniGameUIFactory] StrawberryMiniGamePanel 생성 완료");
            return panel;
        }

        /// <summary>
        /// SugarPouringMiniGame 컴포넌트를 추가하고 모든 UI 참조를 자동으로 연결합니다.
        /// </summary>
        /// <param name="panel">Panel GameObject</param>
        /// <returns>설정된 SugarPouringMiniGame 컴포넌트</returns>
        public static SugarPouringMiniGame SetupSugarMiniGameComponent(GameObject panel)
        {
            if (panel == null)
            {
                Debug.LogError("[MiniGameUIFactory] Panel이 null입니다!");
                return null;
            }

            // SugarPouringMiniGame 컴포넌트 추가
            SugarPouringMiniGame miniGame = panel.GetComponent<SugarPouringMiniGame>();
            if (miniGame == null)
            {
                miniGame = panel.AddComponent<SugarPouringMiniGame>();
            }

            // UI 참조 자동 찾기 및 연결
            // Reflection을 사용하여 SerializeField에 할당
            var componentType = typeof(SugarPouringMiniGame);
            var fields = componentType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // SugarFill Image
            Transform sugarFill = panel.transform.Find("Container/SugarFill");
            if (sugarFill != null)
            {
                var field = System.Array.Find(fields, f => f.Name == "sugarFillImage");
                if (field != null)
                {
                    field.SetValue(miniGame, sugarFill.GetComponent<Image>());
                }
            }

            // Timer Text
            Transform timerText = panel.transform.Find("TimerText");
            if (timerText != null)
            {
                var field = System.Array.Find(fields, f => f.Name == "timerText");
                if (field != null)
                {
                    field.SetValue(miniGame, timerText.GetComponent<TextMeshProUGUI>());
                }
            }

            // Instructions Text
            Transform instructionsText = panel.transform.Find("InstructionsText");
            if (instructionsText != null)
            {
                var field = System.Array.Find(fields, f => f.Name == "instructionsText");
                if (field != null)
                {
                    field.SetValue(miniGame, instructionsText.GetComponent<TextMeshProUGUI>());
                }
            }

            // Result Panel
            Transform resultPanel = panel.transform.Find("ResultPanel");
            if (resultPanel != null)
            {
                var field = System.Array.Find(fields, f => f.Name == "resultPanel");
                if (field != null)
                {
                    field.SetValue(miniGame, resultPanel.gameObject);
                }
            }

            // Result Text
            Transform resultText = panel.transform.Find("ResultPanel/ResultText");
            if (resultText != null)
            {
                var field = System.Array.Find(fields, f => f.Name == "resultText");
                if (field != null)
                {
                    field.SetValue(miniGame, resultText.GetComponent<TextMeshProUGUI>());
                }
            }

            // Retry Button
            Transform retryButton = panel.transform.Find("ResultPanel/RetryButton");
            if (retryButton != null)
            {
                var field = System.Array.Find(fields, f => f.Name == "retryButton");
                if (field != null)
                {
                    field.SetValue(miniGame, retryButton.GetComponent<Button>());
                }
            }

            // Continue Button
            Transform continueButton = panel.transform.Find("ResultPanel/ContinueButton");
            if (continueButton != null)
            {
                var field = System.Array.Find(fields, f => f.Name == "continueButton");
                if (field != null)
                {
                    field.SetValue(miniGame, continueButton.GetComponent<Button>());
                }
            }

            // Target Zone Image
            Transform targetZone = panel.transform.Find("TargetZone");
            if (targetZone != null)
            {
                var field = System.Array.Find(fields, f => f.Name == "targetZoneImage");
                if (field != null)
                {
                    field.SetValue(miniGame, targetZone.GetComponent<Image>());
                }
            }

            Debug.Log("[MiniGameUIFactory] SugarPouringMiniGame 컴포넌트 설정 완료");
            return miniGame;
        }

        // 공유 Sprite 캐시 (성능 최적화)
        private static Sprite _defaultSprite = null;

        /// <summary>
        /// Image에 기본 Sprite를 할당합니다.
        /// </summary>
        public static void SetDefaultSprite(Image image)
        {
            // 캐시된 Sprite가 없으면 생성
            if (_defaultSprite == null)
            {
                // 1x1 흰색 텍스처로 Sprite 생성
                Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _defaultSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
                _defaultSprite.name = "DefaultUISprite";
            }
            image.sprite = _defaultSprite;
        }

        /// <summary>
        /// TextMeshProUGUI 컴포넌트를 가진 GameObject를 생성합니다.
        /// </summary>
        private static GameObject CreateTextMeshPro(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(200, 50);

            TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
            text.text = "";
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            // 기본 폰트 설정
            if (TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }

            return obj;
        }

        /// <summary>
        /// Button 컴포넌트를 가진 GameObject를 생성합니다.
        /// </summary>
        private static GameObject CreateButton(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(200, 60);

            Image buttonImage = obj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            SetDefaultSprite(buttonImage);

            Button button = obj.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Button Text 생성
            GameObject textObj = CreateTextMeshPro("Text", obj.transform);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            TextMeshProUGUI buttonText = textObj.GetComponent<TextMeshProUGUI>();
            buttonText.text = "Button";
            buttonText.fontSize = 24;
            buttonText.color = Color.white;

            return obj;
        }
    }
}

