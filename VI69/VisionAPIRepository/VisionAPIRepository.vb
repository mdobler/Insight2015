Imports System.ServiceModel
Imports Microsoft.VisualBasic.FileIO
Imports System.Xml
Imports System.Xml.Schema
Imports System.Text
Imports System.IO

''' <summary>
''' 
''' </summary>
''' <remarks></remarks>
Public Class VisionAPIRepository
    Private helper As New VisionAPIHelper
    Private service As VisionAPI.DeltekVisionOpenAPIWebServiceSoapClient

    Private database As String
    Private username As String
    Private password As String

    Public Sub New(uri As String, database As String, username As String, password As String, Optional useHTTPS As Boolean = False, Optional windowsUser As String = "", Optional windowsPassword As String = "", Optional windowsDomain As String = "")
        Try
            service = GetService(uri, useHTTPS, windowsUser, windowsPassword, windowsDomain)

            Me.database = database
            Me.username = username
            Me.password = password

        Catch ex As Exception
            Throw New ApplicationException(String.Format("URI is incorrect: {0}", uri), ex)
        End Try
    End Sub

    Public Shared Function IsURLCorrect(uri As String, Optional useHTTPS As Boolean = False, Optional windowsUser As String = "", Optional windowsPassword As String = "", Optional windowsDomain As String = "") As Boolean
        Try
            Dim _service = GetService(uri, useHTTPS, windowsUser, windowsPassword, windowsDomain)

            Dim retval = _service.MyTest
            Return Not String.IsNullOrEmpty(retval)
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Shared Function GetService(uri As String, Optional useHTTPS As Boolean = False, Optional windowsUser As String = "", Optional windowsPassword As String = "", Optional windowsDomain As String = "") As VisionAPI.DeltekVisionOpenAPIWebServiceSoapClient
        Dim _service As VisionAPI.DeltekVisionOpenAPIWebServiceSoapClient

        If useHTTPS Then
            _service = New VisionAPI.DeltekVisionOpenAPIWebServiceSoapClient("DeltekVisionOpenAPIWebServiceSoapSSL")
        Else
            _service = New VisionAPI.DeltekVisionOpenAPIWebServiceSoapClient("DeltekVisionOpenAPIWebServiceSoap")
        End If

        _service.Endpoint.Address = New EndpointAddress(uri)
        If useHTTPS AndAlso Not String.IsNullOrEmpty(windowsUser) Then
            _service.ClientCredentials.Windows.ClientCredential = New Net.NetworkCredential(windowsUser, windowsPassword, windowsDomain)
            _service.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Identification
        End If

        Return _service
    End Function

    Public Shared Function GetAvailableDatabases(uri As String, Optional useHTTPS As Boolean = False, Optional windowsUser As String = "", Optional windowsPassword As String = "", Optional windowsDomain As String = "") As List(Of String)
        Dim retval As New List(Of String)
        Dim _service = GetService(uri, useHTTPS, windowsUser, windowsPassword, windowsDomain)
        Dim _helper As New VisionAPIHelper

        Dim ds As DataSet = _helper.VisionRetValToDataSet(_service.GetDatabases)


        For Each row As DataRow In ds.Tables(0).Rows
            retval.Add(row(0).ToString)
        Next

        Return retval
    End Function

    Public Function GetSystemInfo() As String
        Try
            Dim connInfo As String = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
            Dim retval As String


            retval = service.GetSystemInfo(connInfo)

            Return retval
        Catch ex As Exception
            Throw New Exception("Error requesting system info Vision API", ex)
        End Try
    End Function

    Public Function GetCurrentUserInfo() As String
        Try
            Dim connInfo As String = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
            Dim retval As String


            retval = service.GetCurrentUserInfo(connInfo)

            Return retval
        Catch ex As Exception
            Throw New Exception("Error requesting system info Vision API", ex)
        End Try
    End Function

#Region "Standard Info Center Methods"

    Public Function GetRecordsByKeyAsDataSet(infocenter As VisionInfoCenters, keys As VisionKeyList, Optional recordDetail As RecordDetail = RecordDetail.Empty, Optional RowAccess As Boolean = False, Optional ChunkSize As Integer = 100, Optional tableInfo As XElement = Nothing) As VisionDataSet
        Dim _connInfo As String = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _icstring = ""
        Dim _nextChunk As Integer = 1
        Dim _xmlretval As String
        Dim _recDetailString = ""

        _recDetailString = [Enum].GetName(GetType(RecordDetail), recordDetail)
        If _recDetailString = "Emtpy" Then _recDetailString = ""

        Dim ds As New VisionDataSet

        Do
            If Not String.IsNullOrEmpty(ds.SessionID) Then
                _connInfo = helper.GetVisionConnInfoXML(ds.SessionID)
            Else
                _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
            End If
            _icstring = helper.GetInfoCenterXML(infocenter, RowAccess, _nextChunk, ChunkSize, tableInfo)
            _xmlretval = service.GetRecordsByKey(_connInfo, _icstring, keys.ToString, _recDetailString)

            ds.AppendData(_xmlretval, False)
            _nextChunk += 1
        Loop While ds.LastChunk = False

        Return ds
    End Function

    Public Function GetRecordsByQueryAsDataSet(infocenter As VisionInfoCenters, query As String, Optional recordDetail As RecordDetail = RecordDetail.Empty, Optional RowAccess As Boolean = False, Optional ChunkSize As Integer = 100, Optional tableInfo As XElement = Nothing) As VisionDataSet
        Dim _connInfo As String = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _icstring = ""
        Dim _nextChunk As Integer = 1
        Dim _xmlretval As String
        Dim _recDetailString = ""
        Dim _xmlquery As XElement = <Queries><Query ID="1"><%= query %></Query></Queries>

        _recDetailString = [Enum].GetName(GetType(RecordDetail), recordDetail)
        If _recDetailString = "Emtpy" Then _recDetailString = ""

        Dim ds As New VisionDataSet

        Do
            If Not String.IsNullOrEmpty(ds.SessionID) Then
                _connInfo = helper.GetVisionConnInfoXML(ds.SessionID)
            Else
                _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
            End If
            _icstring = helper.GetInfoCenterXML(infocenter, RowAccess, _nextChunk, ChunkSize, tableInfo)
            _xmlretval = service.GetRecordsByQuery(_connInfo, _icstring, _xmlquery.ToString, _recDetailString)

            ds.AppendData(_xmlretval, False)
            _nextChunk += 1
        Loop While ds.LastChunk = False

        Return ds
    End Function

    Public Function GetRecordsByKeyAsXDocument(infocenter As VisionInfoCenters, keys As VisionKeyList, Optional recordDetail As RecordDetail = RecordDetail.Empty, Optional RowAccess As Boolean = False, Optional ChunkSize As Integer = 100, Optional tableInfo As XElement = Nothing) As XDocument
        Dim _connInfo As String = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _icXML = ""
        Dim _nextChunk As Integer = 1
        Dim _xmlretval As String
        Dim _sessionID As String = ""
        Dim _lastChunk As Boolean = True
        Dim _recDetail = ""
        Dim _message As VisionMessage

        _recDetail = [Enum].GetName(GetType(RecordDetail), recordDetail)
        If _recDetail = "Emtpy" Then _recDetail = ""

        Dim _xdoc As XDocument

        Do
            If Not String.IsNullOrEmpty(_sessionID) Then
                _connInfo = helper.GetVisionConnInfoXML(_sessionID)
            Else
                _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
            End If
            _icXML = helper.GetInfoCenterXML(infocenter, RowAccess, _nextChunk, ChunkSize, tableInfo)
            Dim _x = keys.ToString
            _xmlretval = service.GetRecordsByKey(_connInfo, _icXML, keys.ToString, _recDetail)

            Dim _result = XDocument.Load(New System.IO.StringReader(_xmlretval))

            'check for messages
            If _result.Element("DLTKVisionMessage") IsNot Nothing Then
                _message = New VisionMessage( _
                    _result.Element("DLTKVisionMessage").Element("ReturnCode").ToString, _
                    _result.Element("DLTKVisionMessage").Element("ReturnDesc").ToString, _
                    _result.Element("DLTKVisionMessage").Element("Detail").ToString)

                If Not String.IsNullOrEmpty(_message.ReturnCode) AndAlso _message.ReturnCode <> "1" Then
                    Throw New DeltekVisionException(_message)
                End If
            End If

            If _result.Element("RECS") IsNot Nothing Then
                _sessionID = _result.Element("RECS").Attribute("SessionID").Value
                If _result.Element("RECS").Attribute("LastChunk") Is Nothing Then
                    _lastChunk = True
                Else
                    _lastChunk = CBool(_result.Element("RECS").Attribute("LastChunk").Value)
                End If

                If _xdoc Is Nothing Then _xdoc = _result Else _xdoc.Element("RECS").Add(_result.Element("RECS").Elements)

                _nextChunk += 1
            End If


        Loop While _lastChunk = False

        Return _xdoc
    End Function

    Public Function GetRecordsByQueryAsXDocument(infocenter As VisionInfoCenters, query As String, Optional recordDetail As RecordDetail = RecordDetail.Empty, Optional RowAccess As Boolean = False, Optional ChunkSize As Integer = 100, Optional tableInfo As XElement = Nothing) As XDocument
        Dim _connInfo As String = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _icstring = ""
        Dim _nextChunk As Integer = 1
        Dim _xmlretval As String
        Dim _sessionID As String = ""
        Dim _lastChunk As Boolean = True
        Dim _recDetailString = ""
        Dim _xmlquery As XElement = <Queries><Query ID="1"><%= query %></Query></Queries>
        Dim _message As VisionMessage

        _recDetailString = [Enum].GetName(GetType(RecordDetail), recordDetail)
        If _recDetailString = "Emtpy" Then _recDetailString = ""


        Dim _xdoc As XDocument

        Do
            If Not String.IsNullOrEmpty(_sessionID) Then
                _connInfo = helper.GetVisionConnInfoXML(_sessionID)
            Else
                _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
            End If
            _icstring = helper.GetInfoCenterXML(infocenter, RowAccess, _nextChunk, ChunkSize, tableInfo)
            _xmlretval = service.GetRecordsByQuery(_connInfo, _icstring, _xmlquery.ToString, _recDetailString)

            Dim _result = XDocument.Load(New System.IO.StringReader(_xmlretval))

            'check for messages
            If _result.Element("DLTKVisionMessage") IsNot Nothing Then
                _message = New VisionMessage( _
                    _result.Element("DLTKVisionMessage").Element("ReturnCode").ToString, _
                    _result.Element("DLTKVisionMessage").Element("ReturnDesc").ToString, _
                    _result.Element("DLTKVisionMessage").Element("Detail").ToString)

                If Not String.IsNullOrEmpty(_message.ReturnCode) AndAlso _message.ReturnCode <> "1" Then
                    Throw New DeltekVisionException(_message)
                End If
            End If

            If _result.Element("RECS") IsNot Nothing Then
                _sessionID = _result.Element("RECS").Attribute("SessionID").Value
                If _result.Element("RECS").Attribute("LastChunk") Is Nothing Then
                    _lastChunk = True
                Else
                    _lastChunk = CBool(_result.Element("RECS").Attribute("LastChunk").Value)
                End If

                If _xdoc Is Nothing Then _xdoc = _result Else _xdoc.Element("RECS").Add(_result.Element("RECS").Elements)

                _nextChunk += 1
            End If


        Loop While _lastChunk = False

        Return _xdoc
    End Function

    Public Function SendDataToDeltekVision(infoCenter As VisionInfoCenters, data As XElement) As VisionMessage
        Dim _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _retval = service.SendDataToDeltekVision([Enum].GetName(GetType(VisionInfoCenters), infoCenter), _connInfo, data.ToString)

        Dim _message As VisionMessage = helper.GetVisionMessageFromReturnValue(_retval)

        Return _message
    End Function

#End Region

#Region "UDIC Methods"
    Public Function GetUDICRecordsByKeyAsDataSet(udicName As String, id As String, Optional recordDetail As RecordDetail = RecordDetail.Empty) As VisionDataSet
        Dim _connInfo As String = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _nextChunk As Integer = 1
        Dim _xmlretval As String
        Dim _recDetailString = ""

        _recDetailString = [Enum].GetName(GetType(RecordDetail), recordDetail)
        If _recDetailString = "Emtpy" Then _recDetailString = ""

        Dim ds As New VisionDataSet

        Do
            If Not String.IsNullOrEmpty(ds.SessionID) Then
                _connInfo = helper.GetVisionConnInfoXML(ds.SessionID)
            Else
                _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
            End If

            _xmlretval = service.GetUDICByKey(_connInfo, udicName, id, _recDetailString)

            ds.AppendData(_xmlretval, False)
            _nextChunk += 1
        Loop While ds.LastChunk = False

        Return ds
    End Function

    Public Function GetUDICRecordsByQueryAsDataSet(udicName As String, query As String, Optional recordDetail As RecordDetail = RecordDetail.Empty) As VisionDataSet
        Dim _connInfo As String = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _nextChunk As Integer = 1
        Dim _xmlretval As String
        Dim _recDetailString = ""
        Dim _xmlquery As XElement = <Queries><Query ID="1"><%= query %></Query></Queries>

        _recDetailString = [Enum].GetName(GetType(RecordDetail), recordDetail)
        If _recDetailString = "Emtpy" Then _recDetailString = ""

        Dim ds As New VisionDataSet

        Do
            If Not String.IsNullOrEmpty(ds.SessionID) Then
                _connInfo = helper.GetVisionConnInfoXML(ds.SessionID)
            Else
                _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
            End If
            _xmlretval = service.GetUDICByQuery(_connInfo, udicName, _xmlquery.ToString, _recDetailString)

            ds.AppendData(_xmlretval, False)
            _nextChunk += 1
        Loop While ds.LastChunk = False

        Return ds
    End Function

    Public Function GetUDICRecordsByKeyAsXDocument(udicName As String, id As String, Optional recordDetail As RecordDetail = RecordDetail.Empty) As XDocument
        Dim _connInfo As String = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _nextChunk As Integer = 1
        Dim _xmlretval As String
        Dim _sessionID As String = ""
        Dim _lastChunk As Boolean = True
        Dim _recDetailString = ""
        Dim _message As VisionMessage

        _recDetailString = [Enum].GetName(GetType(RecordDetail), recordDetail)
        If _recDetailString = "Emtpy" Then _recDetailString = ""

        Dim _xdoc As XDocument

        Do
            If Not String.IsNullOrEmpty(_sessionID) Then
                _connInfo = helper.GetVisionConnInfoXML(_sessionID)
            Else
                _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
            End If
            _xmlretval = service.GetUDICByKey(_connInfo, udicName, id, _recDetailString)

            Dim _result = XDocument.Load(New System.IO.StringReader(_xmlretval))

            'check for messages
            If _result.Element("DLTKVisionMessage") IsNot Nothing Then
                _message = New VisionMessage( _
                    _result.Element("DLTKVisionMessage").Element("ReturnCode").ToString, _
                    _result.Element("DLTKVisionMessage").Element("ReturnDesc").ToString, _
                    _result.Element("DLTKVisionMessage").Element("Detail").ToString)

                If Not String.IsNullOrEmpty(_message.ReturnCode) AndAlso _message.ReturnCode <> "1" Then
                    Throw New DeltekVisionException(_message)
                End If
            End If

            If _result.Element("RECS") IsNot Nothing Then
                _sessionID = _result.Element("RECS").Attribute("SessionID").Value
                If _result.Element("RECS").Attribute("LastChunk") Is Nothing Then
                    _lastChunk = True
                Else
                    _lastChunk = CBool(_result.Element("RECS").Attribute("LastChunk").Value)
                End If

                If _xdoc Is Nothing Then _xdoc = _result Else _xdoc.Element("RECS").Add(_result.Element("RECS").Elements)

                _nextChunk += 1
            End If


        Loop While _lastChunk = False

        Return _xdoc
    End Function

    Public Function GetUDICRecordsByQueryAsXDocument(udicName As String, query As String, Optional recordDetail As RecordDetail = RecordDetail.Empty) As XDocument
        Dim _connInfo As String = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _nextChunk As Integer = 1
        Dim _xmlretval As String
        Dim _sessionID As String = ""
        Dim _lastChunk As Boolean = True
        Dim _recDetailString = ""
        Dim _xmlquery As XElement = <Queries><Query ID="1"><%= query %></Query></Queries>
        Dim _message As VisionMessage

        _recDetailString = [Enum].GetName(GetType(RecordDetail), recordDetail)
        If _recDetailString = "Emtpy" Then _recDetailString = ""


        Dim _xdoc As XDocument

        Do
            If Not String.IsNullOrEmpty(_sessionID) Then
                _connInfo = helper.GetVisionConnInfoXML(_sessionID)
            Else
                _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
            End If
            _xmlretval = service.GetUDICByQuery(_connInfo, udicName, _xmlquery.ToString, _recDetailString)

            Dim _result = XDocument.Load(New System.IO.StringReader(_xmlretval))

            'check for messages
            If _result.Element("DLTKVisionMessage") IsNot Nothing Then
                _message = New VisionMessage( _
                    _result.Element("DLTKVisionMessage").Element("ReturnCode").ToString, _
                    _result.Element("DLTKVisionMessage").Element("ReturnDesc").ToString, _
                    _result.Element("DLTKVisionMessage").Element("Detail").ToString)

                If Not String.IsNullOrEmpty(_message.ReturnCode) AndAlso _message.ReturnCode <> "1" Then
                    Throw New DeltekVisionException(_message)
                End If
            End If

            If _result.Element("RECS") IsNot Nothing Then
                _sessionID = _result.Element("RECS").Attribute("SessionID").Value
                If _result.Element("RECS").Attribute("LastChunk") Is Nothing Then
                    _lastChunk = True
                Else
                    _lastChunk = CBool(_result.Element("RECS").Attribute("LastChunk").Value)
                End If

                If _xdoc Is Nothing Then _xdoc = _result Else _xdoc.Element("RECS").Add(_result.Element("RECS").Elements)

                _nextChunk += 1
            End If


        Loop While _lastChunk = False

        Return _xdoc
    End Function

    Public Function AddUDIC(udicName As String, data As XElement) As VisionMessage
        Dim _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _retval = service.AddUDIC(_connInfo, udicName, data.ToString)

        Dim _message As VisionMessage = helper.GetVisionMessageFromReturnValue(_retval)

        Return _message
    End Function

    Public Function UpdateUDIC(udicName As String, data As XElement) As VisionMessage
        Dim _connInfo = helper.GetVisionConnInfoXML(Me.database, Me.username, Me.password)
        Dim _retval = service.UpdateUDIC(_connInfo, udicName, data.ToString)

        Dim _message As VisionMessage = helper.GetVisionMessageFromReturnValue(_retval)

        Return _message
    End Function
#End Region
End Class
