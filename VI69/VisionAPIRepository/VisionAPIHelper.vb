Imports System.Xml
Imports System.Xml.Schema

''' <summary>
''' helper functions
''' </summary>
''' <remarks></remarks>
Public Class VisionAPIHelper
    ''' <summary>
    ''' creates a dataset from an XML input
    ''' </summary>
    ''' <param name="xml"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function VisionRetValToDataSet(xml As String) As DataSet

        Dim ds As New DataSet
        ds.ReadXml(New System.IO.StringReader(xml))

        Return ds
    End Function

    Public Function GetVisionConnInfoXML(sessionId As String) As String
        Dim retval As XElement = <VisionConnInfo>
                                     <SessionID><%= sessionId %></SessionID>
                                 </VisionConnInfo>
        Return retval.ToString
    End Function

    Public Function GetVisionConnInfoXML(database As String, sessionId As String) As String

        Dim retval As XElement = <VisionConnInfo>
                                     <databaseDescription><%= database %></databaseDescription>
                                     <SessionID><%= sessionId %></SessionID>
                                 </VisionConnInfo>
        Return retval.ToString
    End Function

    Public Function GetVisionConnInfoXML(database As String, username As String, password As String) As String
        Dim retval As XElement = <VisionConnInfo>
                                     <databaseDescription><%= database %></databaseDescription>
                                     <userName><%= username %></userName>
                                     <userPassword><%= password %></userPassword>
                                 </VisionConnInfo>
        Return retval.ToString

    End Function

    Public Function GetVisionConnInfoXML(database As String, username As String, password As String, sessionID As String) As String
        Dim retval As XElement = <VisionConnInfo>
                                     <databaseDescription><%= database %></databaseDescription>
                                     <userName><%= username %></userName>
                                     <userPassword><%= password %></userPassword>
                                     <SessionID><%= sessionID %></SessionID>
                                 </VisionConnInfo>
        Return retval.ToString

    End Function

    Public Function GetInfoCenterXML(infoCenter As VisionInfoCenters, Optional RowAccess As Boolean = False, Optional NextChunk As Integer = 0, Optional ChunkSize As Integer = 100, Optional tableInfo As XElement = Nothing) As String
        Dim retval As XElement

        If NextChunk > 0 Then
            retval = <InfoCenters>
                         <InfoCenter
                             ID=<%= CInt(infoCenter) %>
                             Name=<%= [Enum].GetName(GetType(VisionInfoCenters), infoCenter) %>
                             RowAccess=<%= IIf(RowAccess, "1", "0") %>
                             Chunk=<%= NextChunk %>
                             ChunkSize=<%= ChunkSize %>
                             >
                             <%= tableInfo %>
                         </InfoCenter>
                     </InfoCenters>
        Else
            retval = <InfoCenters>
                         <InfoCenter
                             ID=<%= CInt(infoCenter) %>
                             Name=<%= [Enum].GetName(GetType(VisionInfoCenters), infoCenter) %>
                             RowAccess=<%= IIf(RowAccess, "1", "0") %>>
                             <%= tableInfo %>
                         </InfoCenter>
                     </InfoCenters>

        End If
        Return retval.ToString
    End Function

    Public Function GetVisionMessageFromReturnValue(xml As String) As VisionMessage
        Dim doc As XDocument
        Dim retval As New VisionMessage
        doc = XDocument.Load(New System.IO.StringReader(Xml))

        'check for messages
        If doc.Element("DLTKVisionMessage") IsNot Nothing Then
            retval = New VisionMessage( _
                doc.Element("DLTKVisionMessage").Element("ReturnCode").Value, _
                doc.Element("DLTKVisionMessage").Element("ReturnDesc").Value, _
                "")

            If doc.Element("DLTKVisionMessage").Element("Detail") IsNot Nothing Then
                retval.Detail = doc.Element("DLTKVisionMessage").Element("Detail").Value
            End If
        Else
            retval = New VisionMessage
        End If
        Return retval
    End Function


End Class


