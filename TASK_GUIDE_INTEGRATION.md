# 任務指引系統：以 Second Edition 架構整合

## 目前接線

1. `Assets/Resources/TaskGuideAssets/TaskGuideCatalog.asset` 是任務順序清單。前三個任務為前往禮堂、前往中庭、與中庭學生交談。任務資料各自為同資料夾的 `TaskGuideData` asset。新增或調整順序時，在 Unity Inspector 編輯 Catalog 的 `tasks`，不必修改 Manager 程式。
   搬運或移動 `TaskGuideData.cs` 後，必須確認這些任務 asset 的 `m_Script` 指向目前腳本的 GUID；若 Unity Inspector 顯示 Missing Script，Catalog 會載不到任務，左側面板只會顯示「目前沒有任務」。
2. `TaskGuideManager` 於場景載入後建立常駐 `TaskGuideSystem`，讀取 Catalog。當前任務、已完成任務、追蹤狀態都存在 `Assets/Resources/StoryAssets/MainProgress.asset` 的 `GameProgress.flags`，由既有 `SaveProgress`/`LoadProgress` 存檔。讀檔按鈕會呼叫 `RestoreFromProgress()`。
3. `NotebookHUD.prefab` 根物件上的 `TaskGuideHUD` 在遊戲畫面左側建立常駐任務面板，與線索書開關無關；點面板切換追蹤，追蹤時顯示綠色與淡入淡出的狀態文字。線索書地圖頁的 `NotebookMapController` 只建立地圖標記。標記只在追蹤、任務允許顯示、且目前選取的地圖與目標區域相符時顯示。地圖仍遵守既有的已探索區域限制；Editor 的 `Show All Maps For Testing` 可供測試。
4. `TaskGuidePoint` 設在 `Auditorium.unity` 的 `AuditoriumGoalPoint`、`Atrium.unity` 的 `AtriumGoalPoint` 及 `AtriumNpc1Point`。`TaskCompletionWatcher` 只在玩家與指定場景的指定地點 X 距離符合 `reachDistanceX` 時完成 `ReachLocation` 任務。地圖標記優先使用場景任務點的位置，否則使用任務資料的 `targetX01`。
5. `StoryManager` 在對話正常結束時，可由 `StoryStep.reportTaskCompletion` 回報 `TalkToNpc` 任務。按 ESC 中斷不回報。尚未有中庭 NPC 對話物件接上此設定，故第三個任務目前需等 NPC/劇本建立後完成接線。

## 從小地圖開發專案搬運到此架構的步驟

1. 先搬運資料模型及 UI 腳本：`TaskGuideData`、`TaskMarkerIconSet`、`TaskGuidePoint`、`TaskGuidePointRegistry`、`TaskGuideUI`、`TaskGuideTrackingUI`。保留 `.meta` 以維持 GUID。不要搬入舊 `MinimapController` 或 `StoryCoreManager` 的依賴。
2. 將任務 asset 與圖示設定放入 `Assets/Resources/TaskGuideAssets/`。建立 `TaskGuideCatalog.asset`，按遊戲進程填入各 `TaskGuideData`。任務 `taskId` 必須唯一；`targetAreaId` 必須等於 `FullMapRoot` 上 `NotebookMapController.MapArea.areaId`；`targetPointId` 與 `completionTargetId` 依完成方式設定。
3. 在對應場景建立空 GameObject，掛 `TaskGuidePoint`，填 `pointId` 與 `areaId`，把 Transform 放在真正目標位置。`ReachLocation` 任務的 `completionTargetId` 等於該 `pointId`；`reachDistanceX` 設為實際希望的到達容差。
4. 用 `TaskGuideManager` 讀 Catalog、存取 `GameProgress.flags`，並用 `TaskCompletionWatcher` 判斷到達任務。任務切換保留追蹤開關；完成後切到 Catalog 的下一筆。對此專案的存檔讀取，記得在 `LoadProgress()` 後呼叫 `RestoreFromProgress()`。
5. 在 `NotebookHUD.prefab` 根物件掛 `TaskGuideHUD`，讓任務面板成為線索書外的常駐 UI。`FullMapRoot` 的 `NotebookMapController` 只建立地圖標記，沿用已探索地圖的 `MapArea` 與玩家點的 `Left Boundary` / `Right Boundary` 水平座標轉換。任務標記只跟線索書的地圖頁一同顯示，不建立左上角常駐小地圖。
6. NPC 任務要在實際 NPC 使用的 `StoryData` 的最後一個對話 `StoryStep` 勾選 `reportTaskCompletion`，設定 `taskCompletionType = TalkToNpc` 與 `taskCompletionTargetId = AtriumNpc1`。將 `AtriumNpc1Point` 移到 NPC 位置。請勿用接近 NPC 或按互動鍵直接完成；必須等 `StoryManager.OnDialogueComplete(true)`。
7. 在 Unity 中先檢查 Console 無編譯錯誤，再依序測試：進入遊戲場景即看見左側任務面板、開關線索書時面板仍可見、追蹤切換與狀態文字、打開線索書地圖後標記顯隱、切換地圖、玩家移到目標點、任務自動進入下一筆、存檔讀檔後追蹤和當前任務還原、ESC 中斷 NPC 對話不完成、正常結束 NPC 對話才完成。測試完關閉 `Show All Maps For Testing`。

## 目前仍需在 Unity 中確認

- `AuditoriumGoalPoint` 與 `AtriumGoalPoint` 目前放在各場景 X=0，`AtriumNpc1Point` 放在 X=12。設計者需依真正的關卡目標位置調整 Transform，避免玩家進場即達成。
- 中庭場景目前沒有實際 NPC 的 `StoryTrigger` 與劇本接到 `AtriumNpc1`，因此第三個任務尚無遊戲內完成來源。
- 本環境的 Unity batchmode 在取得 LicensingClient 前退出，尚未取得 Unity 編譯與 Play Mode 驗證結果。請以 Unity Editor 的 Console 與上述流程做最終實測。
