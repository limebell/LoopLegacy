using LoopLegacy.UI.Controller;
using LoopLegacy.Loader;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.AddressableAssets;

namespace LoopLegacy.Manager
{
    public class ScriptManager : MonoBehaviour
    {
        public static ScriptManager Instance { get; private set; }
        [SerializeField] private ScriptController _scriptController;
        
        private List<CutsceneDialogue> _currentScript;
        private int _currentDialogueIndex = 0;
        private Action _onScriptComplete;
        private bool _isScriptPlaying = false;
        private Dictionary<string, Sprite> _scriptImages;

        void Awake()
        {
            Instance = this;
        }

        void Oestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 클릭을 처리합니다.
        /// 텍스트가 재생 중이면 완성하고, 완료된 상태면 다음 대사로 넘어갑니다.
        /// </summary>
        public void HandleClick()
        {
            if (_scriptController == null) return;
            
            // 텍스트가 재생 중인지 확인
            if (_scriptController.IsTextPlaying())
            {
                // 텍스트가 재생 중이면 즉시 완성
                _scriptController.CompleteCurrentText();
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
        /// <param name="scriptName">재생할 스크립트 이름</param>
        /// <param name="onComplete">완료 시 실행할 콜백</param>
        public void StartScript(string scriptName, Action onComplete = null, int startIndex = 0)
        {
            // csv 파일 읽어오기
            _currentScript = CutsceneLoader.Load(scriptName);
            
            if (_currentScript == null)
            {
                Debug.LogError($"스크립트 '{scriptName}'을 찾을 수 없습니다.");
                onComplete?.Invoke();
                return;
            }
            
            if (_currentScript.Count == 0)
            {
                Debug.LogWarning($"스크립트 '{scriptName}'에 대사가 없습니다.");
                onComplete?.Invoke();
                return;
            }

            _scriptImages = new Dictionary<string, Sprite>();
            foreach (var script in _currentScript)
            {
                if (script.name != "" && !_scriptImages.ContainsKey(script.name))
                {
                    var sprite = Addressables.LoadAssetAsync<Sprite>(
                        $"Images/NPC/{script.name}").WaitForCompletion();
                    _scriptImages[script.name] = sprite;
                }
            }

            _scriptController.Show();
            
            // 상태 초기화
            _currentDialogueIndex = startIndex;
            _onScriptComplete = onComplete;
            _isScriptPlaying = true;
            
            // 첫 번째 대사 시작
            PlayCurrentDialogue();
        }
        
        /// <summary>
        /// 현재 대사를 재생합니다.
        /// </summary>
        private void PlayCurrentDialogue()
        {
            if (_currentScript == null || _currentDialogueIndex >= _currentScript.Count)
            {
                CompleteScript();
                return;
            }
            
            var dialogue = _currentScript[_currentDialogueIndex];
            
            // HUD에 대사 전달
            if (_scriptController != null)
            {
                // TODO: NPC 이미지와 Background 이미지 처리
                if (_scriptImages.ContainsKey(dialogue.name))
                {
                    _scriptController.SetImage(_scriptImages[dialogue.name]);
                }

                _scriptController.SetName(Utils.GetNPCName(dialogue.name));

                string text = new LocalizedString {
                    TableReference = "Cutscene",
                    TableEntryReference = dialogue.text
                }.GetLocalizedString();
                _scriptController.PlayText(text);
            }
        }
        
        /// <summary>
        /// 다음 대사로 넘어갑니다.
        /// </summary>
        public void NextDialogue()
        {
            if (!_isScriptPlaying) return;

            if (_currentScript[_currentDialogueIndex].pause)
            {
                CompleteScript();
                return;
            }
            
            _currentDialogueIndex++;
            
            if (_currentDialogueIndex >= _currentScript.Count)
            {
                CompleteScript();
            }
            else
            {
                PlayCurrentDialogue();
            }
        }
        
        /// <summary>
        /// 컷신을 완료합니다.
        /// </summary>
        private void CompleteScript()
        {
            _isScriptPlaying = false;
            _onScriptComplete?.Invoke();
            _scriptController.Hide();
            _scriptImages.Clear();
        }
        
        /// <summary>
        /// 컷신을 스킵합니다.
        /// </summary>
        public void SkipScript()
        {
            if (!_isScriptPlaying) return;
            
            CompleteScript();
        }
        
        /// <summary>
        /// 현재 컷신이 재생 중인지 확인합니다.
        /// </summary>
        public bool IsScriptPlaying => _isScriptPlaying;
    }
}
