using LoopLegacy.UI.Controller;
using LoopLegacy.Loader;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace LoopLegacy.Manager
{
    public class CutsceneManager : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private CutsceneController hudController;
        [SerializeField] private AudioClip _cutsceneBGM;
        
        private List<CutsceneDialogue> currentCutscene;
        private int currentDialogueIndex = 0;
        private Action onCutsceneComplete;
        private bool isCutscenePlaying = false;
        
        // Scene 전환 시 전달받을 데이터
        private static string CutsceneToPlay { get; set; } = "initial_cutscene";
        private static Action CutsceneCompleteCallback { get; set; } = () => {
            Debug.Log("컷신 완료");
        };

        void Start()
        {
            /*
            CutsceneToPlay = "Cutscene1";
            CutsceneCompleteCallback = () => {
                Debug.Log("컷신 완료");
            };*/

            // Scene 전환 시 전달받은 데이터가 있으면 컷신 시작
            if (!string.IsNullOrEmpty(CutsceneToPlay))
            {
                StartCutscene(CutsceneToPlay, CutsceneCompleteCallback);
                // 데이터 초기화
                CutsceneToPlay = null;
                CutsceneCompleteCallback = null;
            }

            AudioManager.Instance.PlayBGM(_cutsceneBGM, false);
        }
        
        /// <summary>
        /// 클릭을 처리합니다.
        /// 텍스트가 재생 중이면 완성하고, 완료된 상태면 다음 대사로 넘어갑니다.
        /// </summary>
        public void HandleClick()
        {
            if (hudController == null) return;
            
            // 텍스트가 재생 중인지 확인
            if (hudController.IsTextPlaying())
            {
                // 텍스트가 재생 중이면 즉시 완성
                hudController.CompleteCurrentText();
            }
            else
            {
                // 텍스트 재생이 완료된 상태면 다음 대사로
                NextDialogue();
            }
        }
        
        /// <summary>
        /// 컷신을 시작합니다.
        /// </summary>
        /// <param name="cutsceneName">재생할 컷신 이름</param>
        /// <param name="onComplete">완료 시 실행할 콜백</param>
        public void StartCutscene(string cutsceneName, Action onComplete = null)
        {
            // csv 파일 읽어오기
            currentCutscene = CutsceneLoader.Load(cutsceneName);
            
            if (currentCutscene == null)
            {
                Debug.LogError($"컷신 '{cutsceneName}'을 찾을 수 없습니다.");
                onComplete?.Invoke();
                return;
            }
            
            if (currentCutscene.Count == 0)
            {
                Debug.LogWarning($"컷신 '{cutsceneName}'에 대사가 없습니다.");
                onComplete?.Invoke();
                return;
            }
            
            // 상태 초기화
            currentDialogueIndex = 0;
            onCutsceneComplete = onComplete;
            isCutscenePlaying = true;
            
            // 첫 번째 대사 시작
            PlayCurrentDialogue();
        }
        
        /// <summary>
        /// 현재 대사를 재생합니다.
        /// </summary>
        private void PlayCurrentDialogue()
        {
            if (currentCutscene == null || currentDialogueIndex >= currentCutscene.Count)
            {
                CompleteCutscene();
                return;
            }
            
            var dialogue = currentCutscene[currentDialogueIndex];
            
            // HUD에 대사 전달
            if (hudController != null)
            {
                if (dialogue.image != null)
                {
                    hudController.SetImage(dialogue.image);
                }

                string text = new LocalizedString {
                    TableReference = "Cutscene",
                    TableEntryReference = dialogue.text
                }.GetLocalizedString();
                hudController.PlayText(text);
            }
        }
        
        /// <summary>
        /// 다음 대사로 넘어갑니다.
        /// </summary>
        public void NextDialogue()
        {
            if (!isCutscenePlaying) return;
            
            currentDialogueIndex++;
            
            if (currentDialogueIndex >= currentCutscene.Count)
            {
                CompleteCutscene();
            }
            else
            {
                PlayCurrentDialogue();
            }
        }
        
        /// <summary>
        /// 컷신을 완료합니다.
        /// </summary>
        private void CompleteCutscene()
        {
            isCutscenePlaying = false;
            onCutsceneComplete?.Invoke();
        }
        
        /// <summary>
        /// 컷신을 스킵합니다.
        /// </summary>
        public void SkipCutscene()
        {
            if (!isCutscenePlaying) return;
            
            isCutscenePlaying = false;
            onCutsceneComplete?.Invoke();
        }
        
        /// <summary>
        /// Scene 전환을 통해 컷신을 시작하는 정적 메서드
        /// </summary>
        /// <param name="cutsceneName">재생할 컷신 이름</param>
        /// <param name="onComplete">완료 시 실행할 콜백</param>
        public static void LoadCutsceneScene(string cutsceneName, Action onComplete = null)
        {
            CutsceneToPlay = cutsceneName;
            CutsceneCompleteCallback = onComplete;
            FadeController.LoadScene("Cutscene");
        }
        
        /// <summary>
        /// 현재 컷신이 재생 중인지 확인합니다.
        /// </summary>
        public bool IsCutscenePlaying => isCutscenePlaying;
    }
}
