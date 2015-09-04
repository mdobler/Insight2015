Public Class VisionDataSet
    Inherits DataSet

    'properties to track chunking and session info
    Public Property SessionID As String
    Public Property LastChunk As Boolean = True
    Public Property Message As New VisionMessage

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(xml As String, Optional raiseErrorWhenMessage As Boolean = True)
        Me.New()

        Me.AppendData(xml, raiseErrorWhenMessage)
    End Sub

    Public Sub AppendData(xml As String, Optional raiseErrorWhenMessage As Boolean = True)
        Dim doc As XDocument
        doc = XDocument.Load(New System.IO.StringReader(xml))

        'check for messages
        If doc.Element("DLTKVisionMessage") IsNot Nothing Then
            Me.Message = New VisionMessage( _
                doc.Element("DLTKVisionMessage").Element("ReturnCode").ToString, _
                doc.Element("DLTKVisionMessage").Element("ReturnDesc").ToString, _
                doc.Element("DLTKVisionMessage").Element("Detail").ToString)
        Else
            Me.Message = New VisionMessage
        End If

        If doc.Element("RECS") IsNot Nothing Then
            Me.SessionID = doc.Element("RECS").Attribute("SessionID").Value
            If doc.Element("RECS").Attribute("LastChunk") Is Nothing Then
                Me.LastChunk = True
            Else
                Me.LastChunk = CBool(doc.Element("RECS").Attribute("LastChunk").Value)
            End If

            'loop through all <REC> Elements (tables)
            For Each tbl In doc.Element("RECS").Elements("REC").Elements
                If Not Me.VisionTableInfo.ContainsKey(tbl.Name.ToString) Then
                    Me.VisionTableInfo.Add(tbl.Name.ToString, New TableInfo(tbl.Attribute("name").Value, tbl.Attribute("alias").Value, tbl.Attribute("keys").Value))
                End If

                'create table if not exists
                If Not Me.Tables.Contains(tbl.Name.ToString) Then
                    Me.Tables.Add(tbl.Name.ToString)
                End If
                Dim dt As DataTable = Me.Tables(tbl.Name.ToString)

                'loop through all <ROW> elements  to create columns if they do not exists
                For Each col In tbl.Elements("ROW").Elements
                    If Not dt.Columns.Contains(col.Name.ToString) Then dt.Columns.Add(col.Name.ToString)
                Next

                'set primary key
                Dim pkList As New List(Of DataColumn)
                For Each key As String In tbl.Attribute("keys").Value.Split(CChar(","))
                    pkList.Add(dt.Columns(key))
                Next
                dt.PrimaryKey = pkList.ToArray

                'loop through ROWS again to add values to the row
                For Each row In tbl.Elements("ROW")
                    Dim dr = dt.NewRow
                    For Each value In row.Elements
                        dr(value.Name.ToString) = value.Value.ToString
                    Next
                    dt.Rows.Add(dr)
                Next
            Next
        End If

        Me.AcceptChanges()

        If raiseErrorWhenMessage And Not String.IsNullOrEmpty(Me.Message.ReturnCode) Then
            Throw New DeltekVisionException(Me.Message.ReturnDesc)
        End If

    End Sub

    Public Class TableInfo
        Public Property Name As String = ""
        Public Property [Alias] As String = ""
        Public Property Keys As String = ""

        Public Sub New()
        End Sub

        Public Sub New(name As String, Optional [alias] As String = "", Optional keys As String = "")
            Me.Name = name
            Me.Alias = [alias]
            Me.Keys = keys
        End Sub
    End Class

    Public Property VisionTableInfo As Dictionary(Of String, TableInfo) = New Dictionary(Of String, TableInfo)
End Class





