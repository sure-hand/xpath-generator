Imports Microsoft.Win32
Imports System.Reflection

Class MainWindow
    Dim xsdHandler As New XMLSchemaProcessor
    Dim runMode As New XMLSchemaProcessorMode

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.lblVersion.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString
    End Sub

    Private Sub Exit_Click(sender As Object, e As RoutedEventArgs) Handles btnExit.Click
        Me.Close()
    End Sub


    Private Sub btnGenerateXPath_Click(sender As Object, e As RoutedEventArgs) Handles btnGenerateXPath.Click
        If lbGlobalElements.SelectedItem Is Nothing Then
            MsgBox("No elements selected")
            Exit Sub
        End If
        runMode.showLength = Me.cbShowLength.IsChecked
        runMode.showNillable = Me.cbShowNillable.IsChecked
        runMode.showBaseTypeName = Me.cbShowBaseTypeName.IsChecked
        runMode.showOccurs = Me.cbShowOccurs.IsChecked
        runMode.indicateRepeatble = Me.cbIndicateRepeatble.IsChecked

        xsdHandler.GenerateXPath(lbGlobalElements.SelectedItem.Name, lbGlobalElements.SelectedItem.Namespace, runMode)

        Me.tbXPath.Text = xsdHandler.XPath
        Me.tbXPathErrors.Text = xsdHandler.XPathErrors
        If Me.cbAutoCopyToClipboard.IsChecked Then
            System.Windows.Clipboard.SetText(Me.tbXPath.Text)
        End If
    End Sub

    Private Sub btnCopy_Click(sender As Object, e As RoutedEventArgs) Handles btnCopy.Click
        System.Windows.Clipboard.SetText(Me.tbXPath.Text)
    End Sub

    Private Sub btnOpenXSD_Click(sender As Object, e As RoutedEventArgs) Handles btnOpenXSD.Click
        'Selecting the file
        Dim fldg As New OpenFileDialog
        fldg.Title = "Select Main XSD File"
        fldg.Filter = "XSD Files (*.xsd)|*.xsd|All Files (*.*)|*.*"
        fldg.FilterIndex = 1
        If fldg.ShowDialog() = False Then
            Exit Sub
        Else
            tbSourceXSD.Text = fldg.FileName
            LoadXSD()
        End If

    End Sub

    Private Sub btnReloadXSD_Click(sender As Object, e As RoutedEventArgs) Handles btnReloadXSD.Click
        LoadXSD()
    End Sub

    Private Sub LoadXSD()
        xsdHandler.Load(tbSourceXSD.Text)

        lbGlobalElements.Items.Clear()
        tbXPath.Clear()
        tbXPathErrors.Clear()

        For Each item In xsdHandler.GlobalElements
            lbGlobalElements.Items.Add(item)
        Next
    End Sub

End Class
