# WIFramework<br>
유니티- 친화적으로 유니티- 코딩을 편하게 만들어 주는 라이브러리입니다.

현재 지원 되는 기능<br>
WIBehaviour<br>
  *UIBehaviour 의존성 주입<br>
  *CanvasBase 의존성 주입<br>
  *PanelBase 의존성 주입<br>

WIManager<br>
 *IKeyboardActor interface 를 통해 현재 입력받은 키를 전달.<br>

InputManager<br>
  *GetCurrentInputKeyCode : 현재 입력되고 있는 KeyCode를 얻을 수 있습니다.<br>
  
개발 중<br>
1. CanvasBase, PanelBase, UIBase를 통한 UI 계층화 및 지역화 서포트 (2022-11-22 개발 중)<br>
1-2. WIBehaviour를 대상으로 한 약한 싱글턴 기능. (2022-11-22 테스트중)<br>


기본 구조<br>
  *WIBehaviour : MonoBehaviour를 대체하는 클래스입니다. 구조 구현에 도움을 주는 기능들은 이 클래스를 기반으로 짜여집니다.<br>
  *CanvasBase : Unique한 UI 그룹을 의미하는 클래스 입니다. 씬 상에 여러 인스턴스가 존재하면 안됩니다. 이를 위한 약한 싱글턴 기능이 내장됩니다.<br>
  *PanelBase : Unique하지 않은 UI 그룹을 의미하는 클래스 입니다. 씬 상에 여러 인스턴스가 존재해도 되지만, 같은 CanvasBase 내에는 중복으로 존재할 수 없습니다.<br>
  *UIBase : 중복되는 UI가 여러개 필요할 경우에 사용하는 클래스 입니다. ex) HPBar. <br>
  *CanvasBase 는 생성과 동시에 캐싱되고, 아무데서나 접근 가능합니다.<br>
  *PanelBase 는 자신의 부모 CanvasBase에 캐싱되고, 같은 CanvasBase 내부에서는 아무데서나 접근 가능하지만, 외부에서는 CanvasBase를 통해 접근해야 합니다.<br>
  *UIBase 는 특별히 캐싱해두는 주체가 없습니다. <br>
  ![WIFrameworkStructure](https://user-images.githubusercontent.com/46316988/203210053-d5b6ad36-13f1-44c4-9e00-ff9f65ffd87e.png)
  *구조 개념도 그림... 입니다. <br>
  *Canvas는 Unique 해야 하며, Panel은 자신의 부모 Canvas에 대해서만 Unique 하면 됩니다. UIbase의 경우 자유롭습니다.
  *위의 구조는 강한 연결의 구조가 아닙니다.<br>
  *구조를 수립할때 도움이 될 만할 것 같은 단위로 묶은 추상적인 구조입니다.<br>
  *다만 이 프로젝트 내부의 기능은 모두 위의 구조를 따라서 개발되므로 이외의 사용법에서는 버그가 발생할 소지가 있습니다.<br>
  
