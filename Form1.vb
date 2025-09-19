Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing.Imaging
Imports System.IO
Imports System.IO.Ports
Imports System.Reflection
Imports System.Reflection.Metadata
Imports System.Runtime.InteropServices
Imports System.Security.Permissions
Imports System.Security.Policy
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Forms
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports AForge.Video
Imports AForge.Video.DirectShow
Imports Microsoft.Data.SqlClient
Imports Microsoft.Identity.Extensions
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.Web.WebView2.WinForms
Imports UCBioBSPCOMLib
Imports UCSAPICOMLib
Imports Windows.Win32.UI.Input


Public Class Form1

    Private gFormGb As String
    Private videoDevices As FilterInfoCollection
    Private videoSource As VideoCaptureDevice
    Private capturedImage As Bitmap
    Private gImgViewID As String
    Private gBase64ID As String
    Private gMainYN As String
    Private gImgViewID_F As String
    Private gBase64ID_F As String
    Private gMainYN_F As String
    Private gMemIDX_F As Integer
    Private frmWebcamPreview As Form2
    Private frmFinger As frmFinger
    Private DualForms As DualForm

    Private lastSyncTimestamp As DateTime = DateTime.MinValue
    Private WithEvents syncTimer As New System.Windows.Forms.Timer()

    ' 지문 관련 선언 
    Public WithEvents objUCSAPICOM As New UCSAPI()
    Public objTerminalUserData As ITerminalUserData
    Public objServerUserData As IServerUserData
    Public objAccessLogData As IAccessLogData
    Public objAccessControlData As IAccessControlData
    Public objServerAuthentication As IServerAuthentication
    Public objTerminalOption As ITerminalOption

    'UCBioBSP Object
    Public WithEvents objUCBioBSP As New UCBioBSP()
    Public objDevice As IDevice
    Public objExtraction As IExtraction
    Public objMatching As IMatching
    Public objFPData As IFPData
    Public objFPImage As IFPImage
    Public objFastSearch As IFastSearch

    Public szTextEnrolledFIR As String
    Private binaryEnrolledFIR() As Byte

    'UCBioBSP Object-스마트카드
    Private objSmartCard As ISmartCard   ' RF카드용 선언 
    '지문 품질값 51~75 적절한, 76~100 우수한, 정상적인 매칭을 위해서는 76 이상에 지문사용필요함
    ' VB6의 Long (32비트)은 VB.NET의 Integer (32비트)에 해당함..
    Private gBioAPI_QUALITY As Integer


    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' 현재 윈도우 환경이 듀얼 모니터인지 체크해서 듀얼폼을 두번째 모니터에 띄우기
        ' 현재시점 주석처리 (사용안함)
        'If Screen.AllScreens.Length > 1 Then

        '    DualForms = New DualForm()

        '    Dim secondScreen As Screen = Screen.AllScreens(1)    ' Screen.AllScreens(1) : 컴퓨터에 연결된 두번째 모니터, Screen.AllScreens(0) : 컴퓨터에 연결된 첫번째 모니터

        '    DualForms.StartPosition = FormStartPosition.CenterScreen  ' 화면중앙
        '    DualForms.WindowState = FormWindowState.Maximized   ' 최대화
        '    ' 폼의 위치를 두 번째 화면의 왼쪽 상단 좌표로 설정
        '    DualForms.Location = secondScreen.Bounds.Location
        '    DualForms.Show()
        'End If

        'MessageBox.Show("프로그램 실행!!.", "디버깅", MessageBoxButtons.OK, MessageBoxIcon.Error)

        ' 배포시 주석처리, 테스트시 주석해제
        CheckBox1.Checked = 1
        CheckBox1.Visible = True

        ' 배포시 주석해제, 테스트시 주석처리
        'CheckBox1.Checked = 0
        'CheckBox1.Visible = False


        WebView21.Visible = True
        pnlCSMain.Visible = False
        Await subFormLoad()

        '지문 dll 로드
        subFingerLoad()
        '지문서버 시작
        Finger_Server_Start()


    End Sub
    Private Sub subFingerLoad()

        Try
            '// Create UCSCOM object
            objUCSAPICOM = New UCSAPI()
            objTerminalUserData = objUCSAPICOM.TerminalUserData
            objServerUserData = objUCSAPICOM.ServerUserData
            objAccessLogData = objUCSAPICOM.AccessLogData
            objAccessControlData = objUCSAPICOM.AccessControlData
            objServerAuthentication = objUCSAPICOM.ServerAuthentication
            objTerminalOption = objUCSAPICOM.TerminalOption

            '// Create UCBioBSP object
            objUCBioBSP = New UCBioBSP()
            objDevice = objUCBioBSP.Device
            objExtraction = objUCBioBSP.Extraction
            objMatching = objUCBioBSP.Matching
            objFPData = objUCBioBSP.FPData
            objFPImage = objUCBioBSP.FPImage
            objFastSearch = objUCBioBSP.FastSearch '//지문인증용
            objSmartCard = objUCBioBSP.SmartCard '//RF카드 인식용
            objDevice.Enumerate()  ' 현재 pc에 연결된 지문장치 목록을 가져옴
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Async Function subFormLoad() As Task

        ' 현재 모니터 해상도 가져오기
        Dim screenWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        Dim screenHeight As Integer = Screen.PrimaryScreen.Bounds.Height

        If Config_Load() = False Then
            gFormGb = "C"
        Else
            gFormGb = "W"
        End If

        ' 디비정보 읽어오기 (사용안함, 웹에서 Json으로 받아오기때문에)
        'If Config_Load2() = False Then
        '    MessageBox.Show("시스템 정보 확인 필요!", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Error)
        'End If

        ' 시리얼 포트번호가 있으면 시리얼에 연결한다.
        If IsNumeric(gSeralPortNo) = True Then
            modFunc.ConnectSerialPort("COM" + gSeralPortNo, 9600)
            AddHandler modFunc.serialPort.DataReceived, AddressOf HandleSerialData
        End If


        ' 폼 크기 설정
        Me.Width = screenWidth
        Me.Height = screenHeight
        Me.WindowState = FormWindowState.Maximized

        WebView21.Left = 0
        WebView21.Top = 0

        ' CS 최초 로딩될때는 WebView로 웹포스를 호출해주고 
        Dim url As String = "http://julist.webpos.co.kr/login/"
        Await WebView21.EnsureCoreWebView2Async(Nothing)

        WebView21.Width = Me.ClientSize.Width
        WebView21.Height = Me.ClientSize.Height
        'WebView21.Source = New Uri(url & "?Company_PosNo=" & gPosNo & "&Company_Code=" & gCompanyCode)
        WebView21.Source = New Uri(url)

        ' CS 웹뷰는 폼 로딩될때 자바스크립트로부터 수신받을 준비를 한다.
        RemoveHandler WebView21.WebMessageReceived, AddressOf WebView21_WebMessageReceived
        AddHandler WebView21.WebMessageReceived, AddressOf WebView21_WebMessageReceived

        ' 폼 로딩될때 CS 웹뷰는 자바스크립트에다가 아무 액션도 하지 않는다.
        ' 테스트시 주석해제 , 배포시 주석처리
        'RemoveHandler WebView21.NavigationCompleted, AddressOf WebView_NavigationCompleted
        'AddHandler WebView21.NavigationCompleted, AddressOf WebView_NavigationCompleted

    End Function
    ' 시리얼 포트에서 데이터가 수신될 때 호출될 이벤트 핸들러
    Private Sub HandleSerialData(sender As Object, e As SerialDataReceivedEventArgs)

        Me.Invoke(Sub()
                      Try
                          ' 데이터가 완전히 도착할 시간을 확보하기 위해 대기시간 
                          Thread.Sleep(50) '50ms

                          Dim receivedData As String = modFunc.serialPort.ReadExisting()

                          ' 받은 바코드 값 처리
                          Dim barcodeValue As String = receivedData.Trim()
                          If Not String.IsNullOrEmpty(barcodeValue) Then
                              TextBox1.Text = $"읽은 바코드: {barcodeValue}"
                          End If
                      Catch ex As Exception
                          MessageBox.Show("바코드 데이터 수신 오류: " & ex.Message)
                      End Try
                  End Sub)
    End Sub
    Private Async Sub WebView_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs)
        Try

            Dim sPosNo As String = gPosNo.Replace("'", "\'")
            'Dim sPosNo As String = $"포스번호 : {gPosNo}".Replace("'", "\'")  ' 포스번호 : 99

            Dim jsCode As String = $"

                //$.fnIsLogin_test('a'); 

                document.getElementById('id').value = 'test01';
                document.getElementById('pwd').value = 'as1234';
                document.getElementById('txtPosNoRecv').value = '{sPosNo}';

                // 자동로그인 하는법 시작 
                // 관리자 라디오 버튼 강제 선택
                //var adminRadio = document.querySelector('input[name=admin_type][value=A]');
                //if (adminRadio) adminRadio.checked = true;
            
                // 로그인 함수 호출
                 $.fnIsLogin(); 
                // 자동로그인 하는법 끝 

                document.querySelector('a[onclick]').click();
            "

            ' JavaScript 코드 실행
            Await WebView21.CoreWebView2.ExecuteScriptAsync(jsCode)

            ' WebMessageReceived 이벤트는 WebView2가 로드된 후 등록해야 함
            AddHandler WebView21.WebMessageReceived, AddressOf WebView21_WebMessageReceived  ' html에서 넘어온 값 받는 메소드 

        Catch ex As Exception
            MessageBox.Show("자바스크립트 실행 중 오류 발생: " & ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' WebView2로 JSON 데이터 전송하는 함수 (수정)
    Private Async Function SendToWebView2(inputId As String, inputValue As String) As Task
        Try
            ' JavaScript 문자열을 생성하여 해당 input 요소의 값을 설정
            Dim jsCode As String = $"document.getElementById('{inputId}').value = '{inputValue.Replace("'", "\'")}';"

            ' 디버깅용 메시지
            'MessageBox.Show("실행된 JavaScript 코드: " & jsCode, "디버깅", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' HTML에 값 전달
            Await WebView21.ExecuteScriptAsync(jsCode)
        Catch ex As Exception
            MessageBox.Show("텍스트 전달 중 오류 발생: " & ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Function
    Private Async Sub WebView21_WebMessageReceived(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs)
        '자바스크립트에서 받는부분 
        Try
            Dim receivedJson As String = e.WebMessageAsJson

            ' JSON이 문자열로 감싸져 있으면, 파싱하여 JSON 객체로 변환
            If receivedJson.StartsWith("""") AndAlso receivedJson.EndsWith("""") Then
                receivedJson = JsonDocument.Parse(receivedJson.Trim(""""c)).RootElement.GetRawText()
            End If

            Dim doc As JsonDocument = JsonDocument.Parse(receivedJson)
            Dim data As JsonElement = doc.RootElement

            If data.TryGetProperty("call", Nothing) Then
                Dim methodName = data.GetProperty("call").GetString()

                Select Case methodName

                    Case "fnJava_Post"
                        Await fnJava_Post()

                    Case "Get_DBInfo"   ' 로그인 메인 페이지의 fnWebCsDbInfoSetter 함수
                        If data.TryGetProperty("dbInfo", Nothing) Then
                            Dim dbInfoJson As String = data.GetProperty("dbInfo").GetRawText()
                            Await Get_DBInfo(dbInfoJson)    '웹으로부터 디비접속정보를 Json 문자열로 받아서 전역변수에 담는 함수

                            ' 디비접속정보를 가져왔다면 지문 테이블에서 데이타 가져와서  objFastSearch 모듈에 지문 탬플릿 데이타 등록을 진행한다.
                            LoadAllFingerprintsFromDB()  ' 사용자 지문인증을 위한 유니온 고속검색엔진dll에 지문탬플릿을 로드하는 작업 
                            lastSyncTimestamp = DateTime.Now.AddSeconds(-5)
                            ' 갱신 지문데이타 있는지 체크하는 타이머 실행 
                            syncTimer.Interval = 30000   ' 30초
                            'syncTimer.Interval = 10000   ' 10초
                            syncTimer.Start()
                        End If

                    Case "Get_WebPosInfo"
                        Await Get_WebPosInfo() '웹포스가 설치된 pc의 ini 파일을 호출하는 함수

                    Case "Get_WebCamCall"
                        Dim sImgViewID As String = ""
                        Dim sBase64ID As String = ""
                        Dim sMainYn As String = ""
                        If data.TryGetProperty("imgViewID", Nothing) Then
                            sImgViewID = data.GetProperty("imgViewID").GetString()
                        End If
                        If data.TryGetProperty("base64ID", Nothing) Then
                            sBase64ID = data.GetProperty("base64ID").GetString()
                        End If
                        If data.TryGetProperty("MainYn", Nothing) Then
                            sMainYn = data.GetProperty("MainYn").GetString()
                        End If
                        Get_WebCamCall(sImgViewID, sBase64ID, sMainYn)   ' 웹캠 호출

                    Case "Get_FingerRegCall"
                        Dim sImgViewID As String = ""
                        Dim sBase64ID As String = ""
                        Dim sMainYn As String = ""
                        Dim sMemIDX As Integer = -1
                        If data.TryGetProperty("imgViewID", Nothing) Then
                            sImgViewID = data.GetProperty("imgViewID").GetString()
                        End If
                        If data.TryGetProperty("base64ID", Nothing) Then
                            sBase64ID = data.GetProperty("base64ID").GetString()
                        End If
                        If data.TryGetProperty("MainYn", Nothing) Then
                            sMainYn = data.GetProperty("MainYn").GetString()
                        End If
                        If data.TryGetProperty("MemIDX", Nothing) Then
                            sMemIDX = data.GetProperty("MemIDX").GetString()
                        End If
                        Get_FingerRegCall(sImgViewID, sBase64ID, sMainYn, sMemIDX)   ' 지문등록 화면

                    Case "Bas_ConfigLoad"
                        Await Bas_ConfigLoad() '단지코드와 포스번호를 입력받는 설정창을 표시해주는 함수

                    Case "Get_WebJson"   ' 웹에서 json 문자열 통으로 받아오기 
                        If data.TryGetProperty("jsonParam", Nothing) Then
                            Dim jsonString As String = data.GetProperty("jsonParam").GetString()
                            Await Get_WebJson(jsonString)
                        End If

                    Case "Get_TestParameter"   ' 자바스크립트에서 파라메타 넘겨서 받아오는 함수 테스트 
                        ' 파라메타 받아오기
                        Dim sParam1 As String = ""
                        Dim sParam2 As String = ""
                        If data.TryGetProperty("posNo", Nothing) Then
                            sParam1 = data.GetProperty("posNo").GetString()
                        End If
                        If data.TryGetProperty("companyCode", Nothing) Then
                            sParam2 = data.GetProperty("companyCode").GetString()
                        End If
                        Await Get_TestParameter(sParam1, sParam2)

                    Case Else
                        MessageBox.Show("알 수 없는 call 메서드: " & methodName)
                End Select
            End If


            'html id, value 값을 가져옴  <input type="text" name="txtPosNoSend" id="txtPosNoSend">
            If data.TryGetProperty("id", Nothing) AndAlso data.TryGetProperty("value", Nothing) Then
                Dim inputId As String = data.GetProperty("id").GetString()
                Dim inputValue As String = data.GetProperty("value").GetString()
                If inputId = "txtPosNoSend" Then
                    If inputValue.Trim <> gPosNo Then
                        MessageBox.Show("설정되어있는 포스번호 : " & gPosNo & " 가 아닙니다.", "디버깅", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Else
                        MessageBox.Show("설정되어있는 포스번호 : " & gPosNo & " 가 맞습니다.", "디버깅", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("메시지 수신 중 오류 발생: " & ex.Message & vbCrLf & "받은 데이터: " & e.WebMessageAsJson,
                            "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Public Async Function Get_DBInfo(ByVal jsonStr As String) As Task
        Try
            Using doc As JsonDocument = JsonDocument.Parse(jsonStr)
                Dim root As JsonElement = doc.RootElement

                gServer = GetJsonString(root, "server")
                gDatabase = GetJsonString(root, "database")
                gUser = GetJsonString(root, "user")
                gPass = GetJsonString(root, "pass")

                '전역변수에 담긴 디비정보로 디비접속문자열 생성
                modDBConn.ConnectionString = $"Data Source={gServer};Initial Catalog={gDatabase};User ID={gUser};Password={gPass};TrustServerCertificate=True"

                ' MessageBox.Show($"DB 정보 수신 완료:" & vbCrLf &
                '                 $"Server: {gServer}" & vbCrLf &
                '                 $"Database: {gDatabase}" & vbCrLf &
                '                 $"User: {gUser}", "DB 정보 설정 완료")

            End Using
        Catch ex As Exception
            MessageBox.Show("DB 정보 처리 중 오류 발생: " & ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Function
    Public Async Function Get_WebJson(ByVal jsonStr As String) As Task

        Try
            Using doc As JsonDocument = JsonDocument.Parse(jsonStr)
                Dim root As JsonElement = doc.RootElement

                Dim intResult As String = GetJsonString(root, "intResult", True)
                Dim strResult As String = GetJsonString(root, "strResult")
                Dim strNull As String = GetJsonString(root, "strNull", defaultValue:="(null)")  ' 받는 파라메터 변수명(defaultValue) 을 명시적으로 선언해서 필요없는 인자를 생략한다.

                MessageBox.Show($"Get_WebJson 호출됨:" & vbCrLf &
                            $"intResult: {intResult}" & vbCrLf &
                            $"strResult: {strResult}" & vbCrLf &
                            $"strNull: {strNull}", "CS 제이슨 수신 완료!")
            End Using
        Catch ex As Exception
            MessageBox.Show("Get_WebJson 처리 오류: " & ex.Message, "오류")
        End Try
    End Function
    Private Function Config_Load() As Boolean

        Config_Load = True
        Try
            gAppPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), INI_FILENAME)
            gPosNo = GetIni("Settings", "PosNo", gAppPath)
            gCompanyCode = GetIni("Settings", "CompanyCode", gAppPath)
            gSeralPortNo = GetIni("Settings", "SerialPortNo", gAppPath)

            If IsNumeric(gPosNo) = False Or gCompanyCode = "" Then
                'MessageBox.Show("포스번호가 잘못되었습니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Config_Load = False
            End If
        Catch ex As Exception
            Config_Load = False
        End Try

    End Function
    Private Function Config_Load2() As Boolean
        ' 웹에서 json으로 디비접속정보를 받기때문에 이 함수는 사용안함. 혹시몰라 임시로 남겨둠..

        Config_Load2 = True
        Try
            ' 폼 로딩시 디비정보 전역함수에 저장
            Dim systemPath As String = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)
            gAppPath2 = System.IO.Path.Combine(systemPath, INI_DB_FILENAME)

            Dim server As String = DecryptString(GetIni("DATABASE", "Server", gAppPath2))
            Dim database As String = DecryptString(GetIni("DATABASE", "DBName", gAppPath2))
            Dim user As String = DecryptString(GetIni("DATABASE", "UserID", gAppPath2))
            Dim pass As String = DecryptString(GetIni("DATABASE", "Password", gAppPath2))


            If String.IsNullOrEmpty(server) Then
                server = "175.117.144.57,11433"
                database = "WEB_POS"
                user = "sa"
                pass = "julist1101@nate.com"
                PutIni("DATABASE", "Server", EncryptString(server), gAppPath2)
                PutIni("DATABASE", "DBName", EncryptString(database), gAppPath2)
                PutIni("DATABASE", "UserID", EncryptString(user), gAppPath2)
                PutIni("DATABASE", "Password", EncryptString(pass), gAppPath2)
            End If

            modDBConn.ConnectionString = $"Data Source={server};Initial Catalog={database};User ID={user};Password={pass};TrustServerCertificate=True"

            Config_Load2 = True
        Catch ex As Exception
            Config_Load2 = False
        End Try

    End Function
    Private Async Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click

        If txtPosNo.Text.Trim = "" Or txtCompanyCode.Text.Trim = "" Then
            MessageBox.Show("포스번호와 업체코드를 입력해주세요.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        '입력받은값 ini 저장후 
        gPosNo = txtPosNo.Text.Trim
        gCompanyCode = txtCompanyCode.Text.Trim

        PutIni("Settings", "PosNo", gPosNo, gAppPath)
        PutIni("Settings", "CompanyCode", gCompanyCode, gAppPath)

        '다시 웹뷰 로딩
        Await Get_WebPosInfo()

    End Sub
    Public Async Function fnJava_Post() As Task
        MessageBox.Show("fnJava_Post 호출됨", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Function
    Public Async Function Bas_ConfigLoad() As Task

        ' 웹뷰를 숨기고 단지코드와 포스번호를 입력받는 설정창을 띄운다.
        'WebView21.Visible = False   '웹뷰창을 굳이 숨기지 않는다.
        pnlCSMain.Visible = True
        pnlCSMain.Left = (Me.ClientSize.Width - pnlCSMain.Width) \ 2
        pnlCSMain.Top = (Me.ClientSize.Height - pnlCSMain.Height) \ 2

        gAppPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), INI_FILENAME)
        gPosNo = GetIni("Settings", "PosNo", gAppPath)
        gCompanyCode = GetIni("Settings", "CompanyCode", gAppPath)
        txtPosNo.Text = gPosNo
        txtCompanyCode.Text = gCompanyCode

    End Function
    Public Async Function Get_WebPosInfo() As Task

        WebView21.Visible = True
        pnlCSMain.Visible = False

        ' webpos가 설치된 곳의 ini 파일을 읽어오는 함수 
        gAppPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), INI_FILENAME)
        gPosNo = GetIni("Settings", "PosNo", gAppPath)
        gCompanyCode = GetIni("Settings", "CompanyCode", gAppPath)

        ' JavaScript로 문자열 전달 (따옴표 제거)
        Dim sPosNo As String = gPosNo.Replace("'", "\'")
        Dim sCompanyCode As String = gCompanyCode.Replace("'", "\'")

        ' 웹에다가 단지코드와 포스번호를 받는 함수(fnCsWebPosInfoGetter)에 던진다.
        Dim jsCode As String = $"$.fnCsWebPosInfoGetter('{sPosNo}', '{sCompanyCode}');"
        Await WebView21.ExecuteScriptAsync(jsCode)

    End Function

    ' Json 문자열 값 꺼내오는 함수 
    Private Function GetJsonString(root As JsonElement, propName As String, Optional asRawText As Boolean = False, Optional defaultValue As String = "") As String
        Dim value As String = defaultValue
        Dim temp As JsonElement

        If root.TryGetProperty(propName, temp) Then
            If asRawText Then
                value = temp.GetRawText()
            ElseIf temp.ValueKind = JsonValueKind.String Then
                value = temp.GetString()
            End If
        End If

        Return value
    End Function
    Public Async Function Get_TestParameter(ByVal sParam1 As String, ByVal sParam2 As String) As Task

        MessageBox.Show("파라메터 받기 테스트:" & vbCrLf &
                    "파라메타1 : " & sParam1 & vbCrLf &
                    "파라메타2 : " & sParam2, "Get_TestParameter 호출됨", MessageBoxButtons.OK, MessageBoxIcon.Information)

    End Function
    Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        ' webpos가 설치된 곳의 ini 파일을 읽어오는 함수 
        gAppPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), INI_FILENAME)
        gPosNo = GetIni("Settings", "PosNo", gAppPath)
        gCompanyCode = GetIni("Settings", "CompanyCode", gAppPath)

        ' JavaScript로 문자열 전달 (따옴표 제거)
        Dim sPosNo = gPosNo.Replace("'", "\'")
        Dim sCompanyCode = gCompanyCode.Replace("'", "\'")

        Dim jsCode = $"$.fnCsWebPosInfoGetter('{sPosNo}', '{sCompanyCode}');"
        Await WebView21.ExecuteScriptAsync(jsCode)   'ExecuteScriptAsync(...) 같은 함수는 Async/Await 필요

    End Sub
    Private Sub Form1_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        frmSplash.Close()
    End Sub
    Private Async Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        Dim str_intResult = "1"
        Dim str_strResult = "정상적으로 처리되었습니다."
        Dim str_strNull As String = Nothing

        Dim jsonStr =
            $"{{""intResult"":{str_intResult},""strResult"":""{str_strResult}"",""strNull"":{If(str_strNull Is Nothing, "null", $"""{str_strNull}""")}}}"
        'Dim JsonStr As String = "{""intResult"":1, ""strResult"":""정상처리"", ""strNull"":null}"

        jsonStr = jsonStr.Replace("""", "\""") ' 따옴표 제거

        ' 최종완성된 json 문자열 웹으로 전송 
        Dim jsCode = $"$.fnCsWebJsonGetter(""{jsonStr}"");"     ' fnCsWebJsonGetter : 자바스크립트에서 웹포스에서 전달한 문자열을 받는 함수명 
        Await WebView21.ExecuteScriptAsync(jsCode)

    End Sub
    ' 웹캠 폼2에서 Base64를 보내는 자바스크립트 함수 호출을 하기위한 전역함수
    Public Async Function SendBase64ToWebView(base64String As String, targetUrl As String) As Task

        ' MessageBox.Show("자바스크립트 호출 성공!!!.", "성공!!", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Try

            ' 배포할때는 특정페이지로 로드하는 부분은 주석처리함 (2025-07-28) = 테스트할때 주석해제 
            'If WebView21.Source Is Nothing OrElse WebView21.Source.ToString() <> targetUrl Then

            '    'MessageBox.Show($"WebView2에 {targetUrl} 로드 시도...", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information)

            '    ' 페이지 로드 완료를 기다릴 TaskCompletionSource 생성
            '    Dim tcs As New TaskCompletionSource(Of Boolean)()

            '    ' NavigationCompleted 이벤트 핸들러 추가
            '    Dim handler As EventHandler(Of CoreWebView2NavigationCompletedEventArgs) = Nothing
            '    handler = Sub(sender As Object, e As CoreWebView2NavigationCompletedEventArgs)
            '                  RemoveHandler WebView21.CoreWebView2.NavigationCompleted, handler ' 이벤트 핸들러 제거
            '                  If e.IsSuccess Then
            '                      tcs.SetResult(True) ' 로드 성공
            '                  Else
            '                      tcs.SetException(New Exception($"페이지 로드 실패: {e.WebErrorStatus}")) ' 로드 실패
            '                  End If
            '              End Sub
            '    AddHandler WebView21.CoreWebView2.NavigationCompleted, handler

            '    ' 새 URL 로드
            '    WebView21.Source = New Uri(targetUrl)

            '    ' 페이지 로드가 완료될 때까지 비동기로 대기
            '    Await tcs.Task
            '    'MessageBox.Show($"{targetUrl} 로드 성공!", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information)
            'End If


            ' 2. 페이지 로드가 완료된 후 JavaScript 함수 호출
            'Dim jsCode As String = $"imgView('{base64String}');"   ' imgView : 자바스크립트에서 받는 함수명
            Dim jsCode As String = $"$.fnWebImgView('{base64String}', '{gBase64ID}', '{gImgViewID}', '{gMainYN}');"   ' jquery imgView : 자바스크립트에서 받는 함수명
            'Dim jsCode As String = $"imgView('{base64String}', '{gImgViewID}');"     ' 일반함수로 처리할때

            If WebView21 IsNot Nothing AndAlso WebView21.CoreWebView2 IsNot Nothing Then
                Await WebView21.CoreWebView2.ExecuteScriptAsync(jsCode)
            Else
                MessageBox.Show("WebView2가 초기화되지 않아 Base64 전송 실패.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Catch ex As Exception
            MessageBox.Show("WebView2로 Base64 전송 중 오류 발생: " & ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Function
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Get_WebCamCall("", "", "")
    End Sub
    '폼2 웹캠화면 호출
    Public Sub Get_WebCamCall(ByVal sImgViewID As String, ByVal sBase64ID As String, ByVal sMainYn As String)
        gImgViewID = sImgViewID
        gBase64ID = sBase64ID
        gMainYN = sMainYn
        If frmWebcamPreview Is Nothing OrElse frmWebcamPreview.IsDisposed Then  ' frmWebcamPreview가 없거나 닫힌 경우
            syncTimer.Stop()  ' 동기화 타이머 중지
            frmWebcamPreview = New Form2(Me)
            AddHandler frmWebcamPreview.FormClosed, Sub(sender, e)
                                                        syncTimer.Start()
                                                        'Console.WriteLine("웹캠 종료. 동기화 타이머를 다시 시작합니다.")
                                                    End Sub
            frmWebcamPreview.TopMost = True  ' 항상위에
            frmWebcamPreview.StartPosition = FormStartPosition.CenterScreen   ' 모니터 중앙
        End If
        frmWebcamPreview.Show()  ' 폼이 이미 열려있으면 다시 열지 않음
        frmWebcamPreview.Activate()  ' 폼을 활성화
        frmWebcamPreview.StartWebcam()  ' 웹캠 시작
    End Sub
    '지문등록화면 호출
    Public Sub Get_FingerRegCall(ByVal sImgViewID As String, ByVal sBase64ID As String, ByVal sMainYn As String, ByVal sMemIDX As Integer)
        gImgViewID_F = sImgViewID
        gBase64ID_F = sBase64ID
        gMainYN_F = sMainYn
        gMemIDX_F = sMemIDX
        If frmFinger Is Nothing OrElse frmFinger.IsDisposed Then  ' frmWebcamPreview가 없거나 닫힌 경우
            syncTimer.Stop()  ' 동기화 타이머 중지
            frmFinger = New frmFinger(Me)
            AddHandler frmFinger.FormClosed, Sub(sender, e)
                                                 syncTimer.Start()
                                                 'Console.WriteLine("지문 등록 종료. 동기화 타이머를 다시 시작합니다.")
                                             End Sub
            frmFinger.TopMost = True  ' 항상위에
            frmFinger.StartPosition = FormStartPosition.CenterScreen   ' 모니터 중앙
        End If
        frmFinger.Show()  ' 폼이 이미 열려있으면 다시 열지 않음
        frmFinger.Activate()  ' 폼을 활성화
        frmFinger.sMemIDX = gMemIDX_F
    End Sub
    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing

        If frmWebcamPreview IsNot Nothing AndAlso Not frmWebcamPreview.IsDisposed Then
            frmWebcamPreview.Close()
            frmWebcamPreview = Nothing
        End If

        If frmFinger IsNot Nothing AndAlso Not frmFinger.IsDisposed Then
            frmFinger.Close()
            frmFinger = Nothing
        End If

        syncTimer.Stop()
        Finger_Server_Stop() ' 지문서버 종료

        ' Serial 포트 연결 해제
        modFunc.DisconnectSerialPort()
        RemoveHandler modFunc.serialPort.DataReceived, AddressOf HandleSerialData

    End Sub
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged

        If CheckBox1.Checked = True Then
            Button2.Visible = True
            Button3.Visible = True
            Button4.Visible = True
            TextBox1.Visible = True
            txtFingerDataLog.Visible = True
        Else
            Button2.Visible = False
            Button3.Visible = False
            Button4.Visible = False
            TextBox1.Visible = False
            txtFingerDataLog.Visible = False
        End If
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click

        Get_FingerRegCall("", "", "", 177)   ' 177 : 테스트용 회원번호


    End Sub
    ' 지문데이타를 DB에서 가져와서 objFastSearch 에 등록하는 함수
    Private Sub LoadAllFingerprintsFromDB()

        Using conn As SqlConnection = modDBConn.GetConnection()
            If conn Is Nothing Then Return

            Dim sql As String = "SELECT F_MEM_IDX, F_FINGER FROM T_MEM_PHOTO WHERE F_FINGER IS NOT NULL AND DATALENGTH(F_FINGER) > 0  " &
                                " AND F_COMPANY_CODE = @F_COMPANY_CODE "
            Using cmd As New SqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@F_COMPANY_CODE", gCompanyCode)  'gCompanyCode
                Try
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim img As Object = reader("F_FINGER")
                            Dim nUserID As Integer

                            If Not Convert.IsDBNull(img) Then
                                Dim lbytTemp As Byte() = DirectCast(img, Byte())
                                nUserID = If(Convert.IsDBNull(reader("F_MEM_IDX")), 0, Convert.ToInt32(reader("F_MEM_IDX")))
                                If nUserID > 0 Then
                                    ' 1. SDK가 인식할 수 있도록 데이터 파싱 및 파일 저장 (기존 로직 유지)
                                    ' 이 부분은 디버깅용도로 사용되므로 실제사용시에는 필요없기때문에 주석처리한다.
                                    'objFPData.Export(lbytTemp, 400)

                                    'If objUCBioBSP.ErrorCode = 0 Then
                                    '    Dim nFingerCnt As Integer = objFPData.TotalFingerCount
                                    '    Dim nSampleNum As Integer = objFPData.SampleNumber
                                    '    For f As Integer = 0 To nFingerCnt - 1
                                    '        Dim nFingerID As Integer = objFPData.FingerID(f)
                                    '        For s As Integer = 0 To nSampleNum - 1
                                    '            Dim biTemplate As Byte() = objFPData.FPSampleData(nFingerID, s)
                                    '            Dim szFileName As String = Path.Combine(Application.StartupPath, nUserID.ToString() & ".uct")
                                    '            SaveImageFromDb(biTemplate, szFileName)
                                    '        Next
                                    '    Next
                                    'Else
                                    '    ' Export 오류 발생 시 콘솔에 로그를 남깁니다.
                                    '    Console.WriteLine("FPData.Export Error: " & objUCBioBSP.ErrorDescription)
                                    'End If

                                    ' 2. 고속 검색 엔진에 지문 정보 등록 (이 부분이 중요....지문검색엔진 모듈객체에 등록을 해야 인증을 할때 얘를 갖다가 비교를 한다.)
                                    objFastSearch.RemoveUser(nUserID) ' 기존 정보가 있다면 삭제
                                    objFastSearch.AddFIR(lbytTemp, nUserID) ' 새 정보 추가

                                    If objUCBioBSP.ErrorCode <> 0 Then ' UCBioAPIERROR_NONE = 0
                                        MessageBox.Show(objUCBioBSP.ErrorDescription & " [" & objUCBioBSP.ErrorCode & "]")
                                        Return
                                    End If
                                End If
                            End If
                        End While
                    End Using
                Catch ex As Exception
                    MessageBox.Show("데이터 처리 중 오류 발생: " & ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using
        End Using ' 이 지점에서 conn 객체는 자동으로 Close 및 Dispose 됩니다.
    End Sub
    Private Sub Finger_Server_Start()
        Try
            objUCSAPICOM.ServerStart(20, 9870)   ' 9870은 지문인증장비 기본포트값인데 실제 지문인증장비에서 설정된 포트값과 같아야함
            If objUCSAPICOM.ErrorCode <> 0 Then
                MessageBox.Show("지문장비 초기화 중 오류 발생: " & objUCSAPICOM.ErrorCode, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    Private Sub Finger_Server_Stop()
        Try
            objUCSAPICOM.ServerStop()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    Private Sub objUCSAPICOM_EventVerifyFinger1toN(TerminalID As Integer, AuthMode As Integer, InputIDLength As Integer, SecurityLevel As Integer, AntipassbackLevel As Integer, FingerData As Object) Handles objUCSAPICOM.EventVerifyFinger1toN

        '//ErrorCode 설명
        '// 769:미등록사용자, 770:매칭실패, 771:권한없음, 772:지문Capture 실패
        '// 773:인증실패, 774:패스백
        '// 775:권한없음(네트워크 문제로 서버로부터 응답없음)
        '// 776:권한없음(서버가 Busy 상태로 인증을 수행 할수 없음)
        '// 777:얼굴이 인지되지 않았습니다.

        Try

            '// --- 지문 단일 인증 (1:N 매칭) 로직 시작 ---

            ' Variant(Object)로 받은 지문 데이터를 Byte 배열로 명시적 형변환
            Dim fingerDataBytes As Byte() = DirectCast(FingerData, Byte())
            Dim logMessage As String = $"[{DateTime.Now:HH:mm:ss}] FIR 변환 시작"

            ' FIR(지문 템플릿)로 변환
            txtFingerDataLog.Text = logMessage & Environment.NewLine & txtFingerDataLog.Text
            objFPData.Import(1, 1, 2, 400, 400, fingerDataBytes, 0)

            Dim biInputFingerData As Byte() = DirectCast(objFPData.FIR, Byte())
            logMessage = $"[{DateTime.Now:HH:mm:ss}] FIR 변환 완료"
            txtFingerDataLog.Text = logMessage & Environment.NewLine & txtFingerDataLog.Text

            ' 메모리에 로드된 지문 정보와 비교하여 사용자 검색
            objFastSearch.MaxSearchTime = 0 ' 0 = 검색제한시간 : 무제한
            logMessage = $"[{DateTime.Now:HH:mm:ss}] 사용자 검색 시작"
            txtFingerDataLog.Text = logMessage & Environment.NewLine & txtFingerDataLog.Text

            objFastSearch.IdentifyUser(biInputFingerData, UCBioAPI_FIR_SECURITY_LEVEL_NORMAL)
            logMessage = $"[{DateTime.Now:HH:mm:ss}] 사용자 검색 완료"
            txtFingerDataLog.Text = logMessage & Environment.NewLine & txtFingerDataLog.Text

            Dim isAuthorized As Integer
            Dim isAcessibility As Integer = 1
            Dim isVistor As Integer = 0
            Dim isUserID As Integer
            Dim sErrorCode As Integer

            If objUCBioBSP.ErrorCode = 0 Then
                ' 매칭 성공
                Dim objMatchedFpInfo As ITemplateInfo = objFastSearch.MatchedFpInfo

                If objUCBioBSP.ErrorCode = 0 Then
                    isUserID = objMatchedFpInfo.UserID
                    isAuthorized = 1 ' 인증 성공
                    sErrorCode = 0

                    logMessage = $"[{DateTime.Now:HH:mm:ss}] 1차 인증 성공: UserID({isUserID}), FingerID({objMatchedFpInfo.FingerID}) 찾음"
                    txtFingerDataLog.Text = logMessage & Environment.NewLine & txtFingerDataLog.Text

                    ' 여기에서 비지니스 로직을 수행하거나 아니면 웹으로 인증결과만 넘겨주고 웹의 방문창을 띄우게 하거나...
                    ' -------------------비즈니스 로직(권한 확인) 시작-----------------------
                    If CheckUserAuthorizationFromDB(isUserID) Then
                        isAuthorized = 1 ' 인증 성공
                        sErrorCode = 0
                        logMessage = $"{Environment.NewLine}2차 인증 성공: 사용자 ID({isUserID})는 출입 권한이 있습니다."
                        txtFingerDataLog.Text = logMessage & Environment.NewLine & txtFingerDataLog.Text
                    Else
                        isAuthorized = 0 ' 인증 실패
                        sErrorCode = 771 ' ErrorCode: 권한 없음
                        logMessage = $"{Environment.NewLine}2차 인증 실패: 사용자 ID({isUserID})는 출입 권한이 없습니다."
                        txtFingerDataLog.Text = logMessage & Environment.NewLine & txtFingerDataLog.Text
                    End If
                    '------------------- 비즈니스 로직(권한 확인) 종료------------------------------
                Else
                    ' 매칭은 성공했으나, 매칭된 정보(UserID)를 가져오는 데 실패한 경우
                    isUserID = 0
                    isAuthorized = 0 ' 인증 실패
                    sErrorCode = objUCBioBSP.ErrorCode   ' 769 ' 미등록 사용자 또는 정보 조회 실패
                    txtFingerDataLog.Text &= $"{Environment.NewLine}매칭 후 정보 조회 실패"
                End If
            Else
                ' 매칭 실패
                isUserID = 0
                isAuthorized = 0 ' 인증 실패
                sErrorCode = objUCBioBSP.ErrorCode
                logMessage = $"{Environment.NewLine}매칭 실패: {objUCBioBSP.ErrorDescription} [{sErrorCode}]"
                txtFingerDataLog.Text = logMessage & Environment.NewLine & txtFingerDataLog.Text
            End If

            ' --- 인증 결과 전송 ---
            txtFingerDataLog.Text &= $"{Environment.NewLine}인증 결과 전송"
            Dim txtEventTime As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

            ' 인증 타입 설정 (단일 인증)
            objServerAuthentication.SetAuthType(0, 1, 0, 0, 1, 0)
            ' 터미널로 최종 인증 결과 전송 -- 전송되면 접점신호가 발생한다.
            objServerAuthentication.SendAuthResultToTerminal(TerminalID, isUserID, isAcessibility, isVistor, isAuthorized, txtEventTime, sErrorCode)

            logMessage &= $"{Environment.NewLine}<--EventVerifyFinger1toN"
            logMessage &= $"{Environment.NewLine}      +ErrorCode: {objUCSAPICOM.ErrorCode}"
            logMessage &= $"{Environment.NewLine}      +TerminalID: {TerminalID}"
            txtFingerDataLog.Text = logMessage & Environment.NewLine & txtFingerDataLog.Text
            txtFingerDataLog.Text = "------------------------------------" & Environment.NewLine & txtFingerDataLog.Text

            'Dim sMsg As String
            'If isAuthorized = 1 Then
            '    sMsg = "오늘도 즐거운 하루 보내세요!!"
            'Else
            '    sMsg = "등록된 지문이 아니다!!!!!!!"
            'End If
            'objUCSAPICOM.SendPrivateMessageToTerminal(0, TerminalID, Len(sMsg), sMsg, 5)  ' 5초간 메시지 표시

        Catch ex As Exception
            Dim logMessage As String = $"[{DateTime.Now:HH:mm:ss}] 프로그램 오류 발생: {ex.Message}"
            txtFingerDataLog.Text = logMessage & Environment.NewLine & txtFingerDataLog.Text
        End Try




    End Sub
    '지문 갱신데이타 존재여부 확인 
    Public Sub SyncNewFingerprints()

        Using conn As SqlConnection = modDBConn.GetConnection()
            If conn Is Nothing Then Return

            ' 새로운 데이터가 있는지 먼저 존재여부만 판단..
            Dim hasNewData As Boolean = False
            Dim checkSql As String = "IF EXISTS (SELECT 1 FROM T_MEM_PHOTO " &
                                 "WHERE F_COMPANY_CODE = @F_COMPANY_CODE AND F_UDATE > @LastSyncTime " &
                                 "AND F_FINGER IS NOT NULL AND DATALENGTH(F_FINGER) > 0) " &
                                 "SELECT 1 ELSE SELECT 0"

            Using checkCmd As New SqlCommand(checkSql, conn)
                checkCmd.Parameters.AddWithValue("@F_COMPANY_CODE", gCompanyCode)
                checkCmd.Parameters.AddWithValue("@LastSyncTime", lastSyncTimestamp)
                If CInt(checkCmd.ExecuteScalar()) = 1 Then
                    hasNewData = True
                End If
            End Using

            '새로운 데이터가 없으면, 원래 로직을 실행하지 않고 즉시 종료.
            If Not hasNewData Then Return

            Dim sql As String = "SELECT F_MEM_IDX, F_FINGER FROM T_MEM_PHOTO " &
                            "WHERE F_COMPANY_CODE = @F_COMPANY_CODE AND F_UDATE > @LastSyncTime " &
                            "AND F_FINGER IS NOT NULL AND DATALENGTH(F_FINGER) > 0"

            Using cmd As New SqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@F_COMPANY_CODE", gCompanyCode)
                cmd.Parameters.AddWithValue("@LastSyncTime", lastSyncTimestamp)

                Try
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim img As Object = reader("F_FINGER")
                            Dim nUserID As Integer
                            If Not Convert.IsDBNull(img) Then
                                Dim lbytTemp As Byte() = DirectCast(img, Byte())
                                nUserID = If(Convert.IsDBNull(reader("F_MEM_IDX")), 0, Convert.ToInt32(reader("F_MEM_IDX")))
                                If nUserID > 0 Then
                                    objFastSearch.RemoveUser(nUserID) ' 기존 정보가 있다면 삭제
                                    objFastSearch.AddFIR(lbytTemp, nUserID) ' 새 정보 추가
                                    If objUCBioBSP.ErrorCode <> 0 Then
                                        'MessageBox.Show(objUCBioBSP.ErrorDescription & " [" & objUCBioBSP.ErrorCode & "]")
                                        Return
                                    End If
                                End If
                            End If
                        End While
                    End Using
                    lastSyncTimestamp = DateTime.Now.AddSeconds(-5)
                Catch ex As Exception
                    MessageBox.Show("지문 정보 갱신 중 오류 발생: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    Private Sub syncTimer_Tick(sender As Object, e As EventArgs) Handles syncTimer.Tick
        SyncNewFingerprints()
    End Sub
End Class
