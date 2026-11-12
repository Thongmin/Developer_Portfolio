using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TabNavigation : MonoBehaviour
{
  // [Header()] => Inspector 창에 보여지는 부분의 제목 표시
  // 사용자 입력을 받을 수 있는 UI 요소에 공통 기능을 구현해둔 추상 클래스 (Button, Toggle, Slider, Scrollbar, Dropdown, InputField)
  // 이 안에 있는 Navigation 기능을 이용.
  [Header("Tab Order (Top to Bottom)")]
  [SerializeField] private List<Selectable> selectables = new List<Selectable>();

  private int currentIndex = 0; // 현재 선택된 요소인덱스 (defualt = 0 (맨 위))

  void Start()
  {
    // List 안에 들어있는 값이 1개라도 있을 경우에 실행
    if (selectables.Count > 0)
    {
      // Select() = 해당 UI요소에 포커스 함 (하이라이트 표시)
      selectables[0].Select();
    }
  }

  void Update()
  {
    CheckUserSelection();
    
    // InputPackage 업데이트 돼서 작동 방식이 달라짐.
    if (Keyboard.current.tabKey.wasPressedThisFrame)    // 탭키를 누르면 포커스 이동 함수 호출
      SelectNext();

    if (Keyboard.current.spaceKey.wasPressedThisFrame)  // 스페이스 키를 누르면 체크표시 혹은 버튼 클릭 함수 호출 (Button, Toggle만)
      ActivateCurrentElement();
  }

  // 사용자가 직접 선택한 UI를 감지하고 currentIndex 업데이트
  private void CheckUserSelection()
  {
    // EventSystem 에서 현재 선택된 GameObject 가져오기
    GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

    if (selectedObject != null)
    {
      // 선택된 GameObject에서 Selectable 컴포넌트 가져오기
      Selectable selectedSelectable = selectedObject.GetComponent<Selectable>();

      if (selectedSelectable != null)
      {
        // Selectables 리스트에서 해당 요소의 인덱스 찾기
        int foundIndex = selectables.IndexOf(selectedSelectable);

        // 리스트에서 찾았고, 현재 인덱스와 다르다면 업데이트
        if (foundIndex >= 0 && foundIndex != currentIndex)
          currentIndex = foundIndex;
      }
    }
  }

  // 포커스 이동하는 함수 
  private void SelectNext()
  {
    // List가 비어있다면 실행 종료
    if (selectables.Count == 0)
      return;

    currentIndex = (currentIndex + 1) % selectables.Count; // 다음 Index를 가르키도록 함. (List에 추가된 요소 개수가 넘어가면 다시 0으로)
    SelectCurrent();
  }

  // currentIndex 에 맞는 List에 포커스 하는 함수
  private void SelectCurrent()
  {
    // currentIndex가 List 개수 범위 내 일때.
    if (currentIndex >= 0 && currentIndex < selectables.Count)
      selectables[currentIndex].Select(); // currentIndex에 해당하는 UI요소 포커스
  }

  // 해당 UI의 버튼 상호작용이 되도록 하는 함수
  private void ActivateCurrentElement()
  {
    // currentIndex가 List 개수 범위 내 일때.
    if (currentIndex > 0 && currentIndex < selectables.Count)
    {
      var current = selectables[currentIndex];  // 현재 currentIndex에 해당하는 UI요소를 가져옴.

      if (current is Toggle toggle)       // 가져온 UI가 토글이면 체크/해제
        toggle.isOn = !toggle.isOn;
      else if (current is Button button)  // 가져온 UI가 버튼이면 해당 버튼에 있는 event를 실행.
        button.onClick?.Invoke();
    }
  }
}
