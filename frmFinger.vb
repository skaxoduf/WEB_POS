Imports System.IO
Imports System.Reflection
Imports Microsoft.Data.SqlClient
Imports Microsoft.Identity.Extensions
Imports UCBioBSPCOMLib
Imports UCSAPICOMLib

Public Class frmFinger

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
    Private parentForm As Form1
    Public sMemIDX As Integer

    Public Sub New(ByVal callingForm As Form1)
        Me.InitializeComponent()
        ParentForm = callingForm
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click

        ' 지문인식 시작

        Try
            ' Window style
            objUCBioBSP.WindowStyle = UCBioAPI_WINDOW_STYLE_INVISIBLE
            objUCBioBSP.FPForeColor = "000000"
            objUCBioBSP.FPBackColor = "FFFFFF"

            objUCBioBSP.FingerWnd = picFingerImg.Handle.ToInt32    ' 픽쳐박스에 지문이미지 표시함...
            'objUCBioBSP.FingerWnd = 0   ' 픽쳐박스에 지문이미지 표시안함

            ' Window 옵션 설정
            objUCBioBSP.WindowOption(UCBioAPI_WINDOW_STYLE_NO_FPIMG) = False
            objUCBioBSP.WindowOption(UCBioAPI_WINDOW_STYLE_NO_TOPMOST) = False
            objUCBioBSP.WindowOption(UCBioAPI_WINDOW_STYLE_NO_WELCOME) = False

            ' 모든 손가락 등록 비활성화
            For i = 0 To 9
                objUCBioBSP.DisableFingerForEnroll(i) = True
            Next

            ' 장치를 닫고 다시 오픈...
            objDevice.Close(UCBioAPI_DEVICE_ID_AUTO_DETECT)
            objDevice.Open(UCBioAPI_DEVICE_ID_AUTO_DETECT)

            TxtMsg.Text &= vbCrLf
            TxtMsg.Text &= "지문 장비 대기 시작 " & Date.Now.ToString("HH:mm:ss") & vbCrLf
            objExtraction.Capture()   ' 지문 캡처 시작    
            TxtMsg.Text &= "지문 장비 대기 종료 " & Date.Now.ToString("HH:mm:ss") & vbCrLf

            If objUCBioBSP.ErrorCode <> UCBioAPIERROR_NONE Then
                MessageBox.Show($"{objUCBioBSP.ErrorDescription} [{objUCBioBSP.ErrorCode}]")
                Return
            End If

            ' byte 배열 형식으로 지문 데이터 가져오기
            Dim biFIR As Byte() = DirectCast(objExtraction.FIR, Byte())
            If biFIR Is Nothing OrElse biFIR.Length = 0 Then
                MessageBox.Show($"지문 데이터를 얻어오지 못했습니다.")
                Return
            End If

            ' DB에 지문 저장 시도
            If SaveFingerprintToDatabase("UPD", sMemIDX, biFIR) Then
                If parentForm IsNot Nothing Then
                    MessageBox.Show($"지문이 등록되었습니다.")
                    Me.Close()  '지문등록이 완료되었다면 창을 닫아준다.
                End If
            Else
                MessageBox.Show($"지문 데이타를 데이타베이스에 등록하는데 실패하였습니다.")
                Return
            End If


            ' 이건 그냥 텍스트박스에 뿌리는용도...주석처리해도됨....
            ' 결과: "1A-2B-3C-4D-..."
            ' byte로 받아온 데이타를 16진수로 변환
            'Dim hexString = BitConverter.ToString(biFIR)
            'TxtMsg.Text &= vbCrLf & "--- Fingerprint Binary (HEX) ---" & vbCrLf
            'TxtMsg.Text &= hexString & vbCrLf
            'TxtMsg.Text &= "----------------------------------" & vbCrLf

            '' byte로 받아온 데이타를 base64로 변환
            'Dim base64String = Convert.ToBase64String(biFIR)
            'TxtMsg.Text &= vbCrLf & "--- Fingerprint Binary (Base64) ---" & vbCrLf
            'TxtMsg.Text &= base64String & vbCrLf
            'TxtMsg.Text &= "------------------------------------" & vbCrLf

            'TxtMsg.Text &= "배열의 상한값: " & UBound(biFIR).ToString() & vbCrLf
            'TxtMsg.Text &= "objExtraction.FIRLength: " & objExtraction.FIRLength.ToString() & vbCrLf
            'TxtMsg.Text &= "지문 처리 성공!!" & Date.Now.ToString("HH:mm:ss") & vbCrLf

        Catch ex As Exception
            'TxtMsg.Text &= "종료 " & Date.Now.ToString("HH:mm:ss") & vbCrLf
            'TxtMsg.Text &= ex.Message & vbCrLf
            MessageBox.Show(ex.Message)
        Finally
            ' 장치 닫기
            objDevice.Close(UCBioAPI_DEVICE_ID_AUTO_DETECT)
        End Try


    End Sub

    Private Sub frmFinger_Load(sender As Object, e As EventArgs) Handles Me.Load

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

            TxtMsg.Text = "UCBioBSP 초기화 성공!!!"
            objDevice.Enumerate()  ' 현재 pc에 연결된 지문장치 목록을 가져옴
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    ''' 지문 데이터를 DB에 저장합니다. action 파라미터에 따라 INSERT 또는 UPDATE를 수행합니다.
    Private Function SaveFingerprintToDatabase(ByVal action As String, ByVal userID As Integer, ByVal fingerprintFeatureData As Byte()) As Boolean

        Using conn As SqlConnection = modDBConn.GetConnection()

            If conn Is Nothing Then Return False

            Dim sql As String = ""
            Dim actionDescription As String = ""

            If action.ToUpper() = "INS" Then
                sql = "INSERT INTO T_FingerMark (AgentCode, MbNo, FMarkImg, FMarkFeat, LastDate, LastTime) " &
                 "VALUES (@AgentCode, @MbNo, @FMarkImg, @FMarkFeat, @LastDate, @LastTime)"
                actionDescription = "추가"
            ElseIf action.ToUpper() = "UPD" Then
                sql = "UPDATE T_MEM_PHOTO SET F_FINGER = @F_FINGER, F_UDATE = @F_UDATE " &
                      "WHERE F_MEM_IDX = @F_MEM_IDX AND F_COMPANY_CODE = @F_COMPANY_CODE "
                actionDescription = "업데이트"
            Else
                MessageBox.Show("잘못된 접근입니다!!!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            Using cmd As New SqlCommand(sql, conn)
                ' 공통 파라미터 설정
                '왜 AddWithValue 대신 Add를 쓸까?:
                'AddWithValue는 편리하지만, 때로는 개발자가 의도하지 않은 방식으로 데이터 타입을 추론하여 성능 저하나 예기치 않은 오류를 발생시킬 수 있습니다.
                '특히 varbinary, nvarchar(MAX)와 같이 크기가 크고 명확한 타입 지정이 중요한 경우에는,
                'Add 메서드를 사용해 "이 데이터는 정확히 varbinary 타입이야" 라고
                '명시해주는 것이 훨씬 더 안정적이고 권장되는 방식입니다.
                cmd.Parameters.AddWithValue("@F_MEM_IDX", userID.ToString())
                'cmd.Parameters.AddWithValue("@F_COMPANY_IDX", "12")
                cmd.Parameters.AddWithValue("@F_COMPANY_CODE", gCompanyCode) 'gCompanyCode
                cmd.Parameters.Add("@F_FINGER", SqlDbType.VarBinary, -1).Value = fingerprintFeatureData
                cmd.Parameters.AddWithValue("@F_UDATE", DateTime.Now)

                If action.ToUpper() = "INS" Then
                    cmd.Parameters.Add("@F_FINGER", SqlDbType.VarBinary, -1).Value = New Byte() {}
                End If

                Try
                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                    Return rowsAffected > 0
                Catch ex As SqlException
                    If action.ToUpper() = "INS" AndAlso (ex.Number = 2627 Or ex.Number = 2601) Then
                        MessageBox.Show($"이미 등록된 사용자 ID({userID})입니다.", $"DB {actionDescription} 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Else
                        MessageBox.Show($"DB {actionDescription} 중 SQL 오류 발생: {ex.Message}", "DB 오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                    Return False
                Catch ex As Exception
                    MessageBox.Show($"DB {actionDescription} 중 알 수 없는 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return False
                End Try
            End Using
        End Using
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()  '닫기
    End Sub

End Class