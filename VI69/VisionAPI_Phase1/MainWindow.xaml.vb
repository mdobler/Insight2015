Class MainWindow 

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim _service As New VisionAPI.DeltekVisionOpenAPIWebServiceSoapClient

        'TIP: using XElement variables lets you write standard XML data in code without having to use strings and formatting
        Dim _connectionInfo As XElement = <VisionConnInfo>
                                              <databaseDescription>VisionDemo74 (On Premise)</databaseDescription>
                                              <userName>ADMIN</userName>
                                              <userPassword></userPassword>
                                          </VisionConnInfo>

        Dim retval = _service.GetSystemInfo(_connectionInfo.ToString)

        MessageBox.Show(retval)
    End Sub
End Class
