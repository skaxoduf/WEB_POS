Imports System.Drawing
Imports System.Drawing.Drawing2D ' InterpolationMode, SmoothingMode, PixelOffsetMode를 위해 필요
Imports System.Drawing.Imaging

Module ImageHelper

    ''' <summary>
    ''' 이미지를 지정된 너비와 높이로 리사이징하고, 품질 설정을 적용합니다.
    ''' </summary>
    ''' <param name="originalImage">원본 Image 객체</param>
    ''' <param name="newWidth">새로운 너비</param>
    ''' <param name="newHeight">새로운 높이</param>
    ''' <returns>리사이징된 Bitmap 객체</returns>
    Public Function ResizeImage(ByVal originalImage As Image, ByVal newWidth As Integer, ByVal newHeight As Integer) As Bitmap
        If originalImage Is Nothing Then
            Return Nothing
        End If

        ' 새 비트맵 생성
        Dim resizedBitmap As New Bitmap(newWidth, newHeight)

        Using graphics As Graphics = Graphics.FromImage(resizedBitmap)
            ' 품질 설정 (선택 사항: 필요에 따라 조정)
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic ' 부드러운 리사이징
            graphics.SmoothingMode = SmoothingMode.HighQuality ' 부드러운 가장자리
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality ' 픽셀 오프셋 품질

            ' 원본 이미지를 새로운 비트맵에 그립니다.
            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight)
        End Using

        Return resizedBitmap
    End Function

    ''' <summary>
    ''' 이미지를 원본 비율을 유지하면서 최대 너비와 높이에 맞춰 리사이징합니다.
    ''' </summary>
    ''' <param name="originalImage">원본 Image 객체</param>
    ''' <param name="maxWidth">허용되는 최대 너비</param>
    ''' <param name="maxHeight">허용되는 최대 높이</param>
    ''' <returns>리사이징된 Bitmap 객체</returns>
    Public Function ResizeImagePreservingAspectRatio(ByVal originalImage As Image, ByVal maxWidth As Integer, ByVal maxHeight As Integer) As Bitmap
        If originalImage Is Nothing Then
            Return Nothing
        End If

        Dim ratioX As Single = CType(maxWidth, Single) / originalImage.Width
        Dim ratioY As Single = CType(maxHeight, Single) / originalImage.Height
        Dim ratio As Single = Math.Min(ratioX, ratioY) ' 너비와 높이 중 더 작은 비율을 선택하여 둘 다 만족시키도록

        Dim newWidth As Integer = CType(originalImage.Width * ratio, Integer)
        Dim newHeight As Integer = CType(originalImage.Height * ratio, Integer)

        ' 비율을 유지하되, 최소 1픽셀은 되도록 보장
        If newWidth = 0 AndAlso originalImage.Width > 0 Then newWidth = 1
        If newHeight = 0 AndAlso originalImage.Height > 0 Then newHeight = 1

        Return ResizeImage(originalImage, newWidth, newHeight)
    End Function

End Module