Public Class VisionWorkflowMessage
    Private _messages As List(Of String) = New List(Of String)
    Private _isError As Boolean = False

    Public Sub AddWarning(message As String)
        _messages.Add(message)
    End Sub

    Public Sub AddError(message As String)
        _messages.Add(message)
        _isError = True
    End Sub

    Public Sub New()
        Me.Clear()
    End Sub

    Public Sub Clear()
        _messages = New List(Of String)
        _isError = False
    End Sub

    Public Overrides Function ToString() As String
        Dim retval As XElement = <errors></errors>

        If Not _isError Then
            retval.Add(New XAttribute("warning", "y"))
        End If

        For Each message In _messages
            Dim xmlerr As XElement = <error><%= message %></error>
            retval.Add(xmlerr)
        Next

        Return retval.ToString()
    End Function
End Class