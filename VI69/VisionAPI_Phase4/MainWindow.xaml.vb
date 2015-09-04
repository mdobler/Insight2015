''' <summary>
''' had to add the SubLevel information because it is a required field for the update process
''' </summary>
''' <remarks></remarks>
Class MainWindow
    Dim _url As String = "http://localhost/Vision74/VisionWS.asmx"
    Dim _dbName As String = "VisionDemo74 (On Premise)"
    Dim _user As String = "ADMIN"
    Dim _pwd As String = ""


    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim _repository As New VisionAPIRepository.VisionAPIRepository(_url, _dbName, _user, _pwd)

        Dim _query As String = String.Format("SELECT * from PR where name like '%{0}%'", txtSearchString.Text)

        Dim _tableInfo As XElement = <PR><WBS1/><WBS2/><WBS3/><Name/><SubLevel/></PR>

        Dim _result = _repository.GetRecordsByQueryAsXDocument(VisionAPIRepository.VisionInfoCenters.Projects, _query,
                                                               VisionAPIRepository.RecordDetail.Primary, False, 1, _tableInfo)

        lstResults.Items.Clear()

        For Each row In _result.Element("RECS").Elements("REC").Elements("PR").Elements("ROW")
            lstResults.Items.Add(String.Format("{0} - {1} - {2}", row.Element("WBS1").Value, row.Element("Name").Value, row.Element("SubLevel").Value))
        Next
    End Sub

    Private Sub lstResults_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles lstResults.SelectionChanged
        If e.AddedItems.Count > 0 Then
            Dim _values = e.AddedItems(0).ToString.Split("-")
            txtWBS1.Text = _values(0).Trim
            txtName.Text = _values(1).Trim
            txtSubLevel.Text = _values(2).Trim
        End If
    End Sub

    Private Sub cmdUpdate_Click(sender As Object, e As RoutedEventArgs) Handles cmdUpdate.Click
        Dim _repository As New VisionAPIRepository.VisionAPIRepository(_url, _dbName, _user, _pwd)
        Dim _data As XElement = <RECS xmlns="http://deltek.vision.com/XMLSchema">
                                    <REC>
                                        <PR name="PR" alias="PR" keys="WBS1,WBS2,WBS3">
                                            <ROW tranType="UPDATE">
                                                <WBS1><%= txtWBS1.Text %></WBS1>
                                                <WBS2></WBS2>
                                                <WBS3></WBS3>
                                                <Name><%= txtName.Text %></Name>
                                                <SubLevel><%= txtSubLevel.Text %></SubLevel>
                                            </ROW>
                                        </PR>
                                    </REC>
                                </RECS>

        Dim _retval = _repository.SendDataToDeltekVision(VisionAPIRepository.VisionInfoCenters.Projects, _data)

        MessageBox.Show(_retval.ReturnDesc)
    End Sub
End Class
