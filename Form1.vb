Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing.Imaging
Imports System.IO
Imports System.IO.Ports
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
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.Web.WebView2.WinForms


Public Class Form1

    Private gFormGb As String
    Private videoDevices As FilterInfoCollection
    Private videoSource As VideoCaptureDevice
    Private capturedImage As Bitmap
    Private gImgViewID As String
    Private gBase64ID As String
    Private gMainYN As String
    Private frmWebcamPreview As Form2
    Private DualForms As DualForm


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
        'CheckBox1.Checked = 1
        'CheckBox1.Visible = True

        ' 배포시 주석해제, 테스트시 주석처리
        CheckBox1.Checked = 0
        CheckBox1.Visible = False


        WebView21.Visible = True
        pnlCSMain.Visible = False
        Await subFormLoad()


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

        ' 디비정보 읽어오기
        If Config_Load2() = False Then
            MessageBox.Show("시스템 정보 확인 필요!", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If

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
            frmWebcamPreview = New Form2(Me)
            frmWebcamPreview.TopMost = True  ' 항상위에
            frmWebcamPreview.StartPosition = FormStartPosition.CenterScreen   ' 모니터 중앙
        End If
        frmWebcamPreview.Show()  ' 폼이 이미 열려있으면 다시 열지 않음
        frmWebcamPreview.Activate()  ' 폼을 활성화
        frmWebcamPreview.StartWebcam()  ' 웹캠 시작
    End Sub
    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing

        If frmWebcamPreview IsNot Nothing AndAlso Not frmWebcamPreview.IsDisposed Then
            frmWebcamPreview.Close()
            frmWebcamPreview = Nothing
        End If

        ' Serial 포트 연결 해제
        modFunc.DisconnectSerialPort()
        RemoveHandler modFunc.serialPort.DataReceived, AddressOf HandleSerialData

    End Sub
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged

        If CheckBox1.Checked = True Then
            Button2.Visible = True
            Button3.Visible = True
            TextBox1.Visible = True
        Else
            Button2.Visible = False
            Button3.Visible = False
            TextBox1.Visible = False
        End If
    End Sub

End Class
