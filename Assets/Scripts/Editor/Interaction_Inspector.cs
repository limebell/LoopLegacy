using LoopLegacy.Region;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(Interaction))]
public class Interaction_Inspector : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        // 기본 필드들 추가
        var typeField = new PropertyField(serializedObject.FindProperty("_type"));
        var triggerField = new PropertyField(serializedObject.FindProperty("_trigger"));
        var positionSaveTriggerField = new PropertyField(serializedObject.FindProperty("_positionSaveTrigger"));
        var signboardField = new PropertyField(serializedObject.FindProperty("_signboard"));
        
        root.Add(triggerField);
        root.Add(positionSaveTriggerField);
        root.Add(signboardField);
                
        root.Add(new Label("Interaction Settings") { style = { fontSize = 14, unityFontStyleAndWeight = UnityEngine.FontStyle.Bold, marginTop = 10, marginBottom = 5 } });
        root.Add(typeField);
        
        // 타입별 필드들을 담을 컨테이너
        var typeSpecificContainer = new VisualElement();
        root.Add(typeSpecificContainer);
        
        // 타입 필드 값이 변경될 때마다 UI 업데이트
        typeField.RegisterValueChangeCallback(evt =>
        {
            UpdateTypeSpecificFields(typeSpecificContainer);
            serializedObject.ApplyModifiedProperties();
        });
        
        // 초기 UI 설정
        UpdateTypeSpecificFields(typeSpecificContainer);
        
        // 전체 UI를 serializedObject에 바인딩
        root.Bind(serializedObject);
        
        return root;
    }
    
    private void UpdateTypeSpecificFields(VisualElement container)
    {
        container.Clear();
        
        serializedObject.Update();
        var typeProperty = serializedObject.FindProperty("_type");
        var currentType = (InteractionType)typeProperty.enumValueIndex;
        
        switch (currentType)
        {
            case InteractionType.Move:
                var mapCodeField = new PropertyField(serializedObject.FindProperty("_mapCode"));
                var positionField = new PropertyField(serializedObject.FindProperty("_position"));
                
                container.Add(mapCodeField);
                container.Add(positionField);
                
                // 개별 필드 바인딩
                mapCodeField.Bind(serializedObject);
                positionField.Bind(serializedObject);
                break;
                
            case InteractionType.NPC:
                var npcCodeField = new PropertyField(serializedObject.FindProperty("_npcType"));
                
                container.Add(npcCodeField);
                
                // 개별 필드 바인딩
                npcCodeField.Bind(serializedObject);
                break;
                
            case InteractionType.Boss:
                var monsterCodeField = new PropertyField(serializedObject.FindProperty("_monsterCode"));
                container.Add(monsterCodeField);
                
                // 개별 필드 바인딩
                monsterCodeField.Bind(serializedObject);
                break;
        }
    }
}