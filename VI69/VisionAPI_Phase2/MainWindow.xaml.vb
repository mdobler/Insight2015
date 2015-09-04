Class MainWindow 

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim _service As New VisionAPI.DeltekVisionOpenAPIWebServiceSoapClient

        'TIP: using XElement variables lets you write standard XML data in code without having to use strings and formatting
        Dim _connectionInfo As XElement = <VisionConnInfo>
                                              <databaseDescription>VisionDemo74 (On Premise)</databaseDescription>
                                              <userName>ADMIN</userName>
                                              <userPassword></userPassword>
                                          </VisionConnInfo>

        'info center information. With ChunkSize=1 we request 1 project per round trip until we get LastChunk = true...
        Dim _infoCenter As XElement = <InfoCenters>
                                          <InfoCenter
                                              ID="1"
                                              Name="Projects"
                                              Chunk="1"
                                              ChunkSize="1">
                                              <PR>
                                                  <WBS1/>
                                                  <Name/>
                                              </PR>
                                          </InfoCenter>
                                      </InfoCenters>

        Dim _query As XElement = <Queries>
                                     <Query>SELECT * from PR where name like '%<%= txtSearchString.Text %>%'</Query>
                                 </Queries>

        'clear previous list results
        lstResults.Items.Clear()

        'loop through all results (only one project per return value requested
        Dim _lastChunk As Boolean = False
        Dim _chunk As Integer = 1
        Dim _sessionID As String = ""
        Dim _result As New XDocument
        Do

            Dim xmldata As String = _service.GetRecordsByQuery(_connectionInfo.ToString, _infoCenter.ToString, _query.ToString, "Primary")

            'add new results to document
            _result = XDocument.Load(New System.IO.StringReader(xmldata))

            'check for session id and last chunk information
            If _result.Element("RECS") IsNot Nothing Then
                _sessionID = _result.Element("RECS").Attribute("SessionID").Value
                If _result.Element("RECS").Attribute("LastChunk") Is Nothing Then
                    _lastChunk = True
                Else
                    _lastChunk = CBool(_result.Element("RECS").Attribute("LastChunk").Value)
                End If

                For Each row In _result.Element("RECS").Elements("REC").Elements("PR").Elements("ROW")
                    lstResults.Items.Add(String.Format("{0} - {1}", row.Element("WBS1").Value, row.Element("Name").Value))
                Next

                'reset connection info to use session id
                _connectionInfo = <VisionConnInfo>
                                      <databaseDescription>VisionDemo74 (On Premise)</databaseDescription>
                                      <SessionID><%= _sessionID %></SessionID>
                                  </VisionConnInfo>

                'update chunk counter
                _chunk += 1
                _infoCenter.Element("InfoCenter").Attribute("Chunk").Value = _chunk

            End If
        Loop While _lastChunk = False






    End Sub
End Class
