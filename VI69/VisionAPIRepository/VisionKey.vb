Public Class VisionKey
    Public SubKeys As New List(Of String)

    Private Sub New()
        SubKeys = New List(Of String)
    End Sub

    Public Shared Function CreateGenericKey(key As String) As VisionKey
        Dim retval As New VisionKey
        retval.SubKeys.Add(key)

        Return retval
    End Function

    Public Shared Function CreateProjectKey(wbs1 As String, Optional wbs2 As String = "", Optional wbs3 As String = "")
        Dim retval As New VisionKey

        retval.SubKeys.Add(wbs1)
        retval.SubKeys.Add(wbs2)
        retval.SubKeys.Add(wbs3)

        Return retval
    End Function

    Public Overrides Function ToString() As String
        Dim retval As XElement
        Dim counter As Integer = 1
        retval = <KeyValues></KeyValues>

        Dim xKey As XElement = <Keys ID="1">
                                   <Key></Key>
                               </Keys>

        For Each subkey In Me.SubKeys
            xKey.Element("Key").Add(<Fld><%= subkey %></Fld>)
        Next

        retval.Add(xKey)

        Return retval.ToString
    End Function
End Class

Public Class VisionKeyList
    Inherits List(Of VisionKey)

    Public Overrides Function ToString() As String
        Dim retval As XElement
        Dim counter As Integer = 1
        retval = <KeyValues></KeyValues>

        For Each key In Me
            Dim xKey As XElement = <Keys ID=<%= counter %>>
                                       <Key></Key>
                                   </Keys>

            For Each subkey In key.SubKeys
                xKey.Element("Key").Add(<Fld><%= subkey %></Fld>)
            Next

            retval.Add(xKey)
            counter += 1
        Next

        Return retval.ToString

    End Function
End Class