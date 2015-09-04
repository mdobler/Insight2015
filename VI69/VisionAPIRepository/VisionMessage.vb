Public Class VisionMessage
    Public Property ReturnCode As String = ""
    Public Property ReturnDesc As String = ""
    Public Property Detail As String = ""

    Public Sub New()
    End Sub

    Public Sub New(code As String, desc As String, detail As String)
        Me.New()
        Me.ReturnCode = code
        Me.ReturnDesc = desc
        Me.Detail = detail
    End Sub

    Public Overrides Function ToString() As String
        Dim xmlret As XElement = <DLTKVisionMessage>
                                     <ReturnCode><%= Me.ReturnCode %></ReturnCode>
                                     <ReturnDesc><%= Me.ReturnDesc %></ReturnDesc>
                                     <Detail><%= Me.Detail %></Detail>
                                 </DLTKVisionMessage>


        Return xmlret.ToString()
    End Function
End Class