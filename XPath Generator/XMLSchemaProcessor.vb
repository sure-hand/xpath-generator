Imports System
Imports System.Collections
Imports System.Xml
Imports System.Xml.Schema

Public Class XMLSchemaProcessor
    Dim schemaSet As XmlSchemaSet
    Dim _XPath As String
    Dim _XPathErrors As String
    Dim XPathStack As New Stack(Of String)
    Dim runMode As XMLSchemaProcessorMode

    Public Sub Load(XMLSchemaPath As String)
        ' Add the schema to a new XmlSchemaSet and compile it.
        ' Any schema validation warnings and errors encountered reading or 
        ' compiling the schema are handled by the ValidationEventHandler delegate.
        schemaSet = New XmlSchemaSet()
        AddHandler schemaSet.ValidationEventHandler, AddressOf ValidationCallback
        schemaSet.Add(Nothing, XMLSchemaPath)
        schemaSet.Compile()
    End Sub

    Public Sub GenerateXPath(name As String, ns As String, mode As XMLSchemaProcessorMode)
        Dim RootElement As XmlSchemaElement
        Dim RootElementName As XmlQualifiedName = New XmlQualifiedName(name, ns)

        XPathStack.Clear()
        _XPath = ""
        _XPathErrors = ""
        runMode = mode
        RootElement = schemaSet.GlobalElements.Item(RootElementName)
        processElement(RootElement, "")
    End Sub

    Private Sub processElement(xsdElement As XmlSchemaElement, currentXPath As String)
        Dim stackElement As String
        Dim sTemp As String
        Dim nillableString As String
        Dim occursString, occursStringMin, occursStringMax As String
        Dim simpleTypeAnalyzer As New XMLSchemaSimpleTypeInfo

        stackElement = xsdElement.QualifiedName.Namespace & ":" & xsdElement.QualifiedName.Name & "(" & xsdElement.ElementSchemaType.GetType.Name & ":" & getTypeName(xsdElement) & ")"
        If XPathStack.Contains(stackElement) Then
            Exit Sub
        Else
            XPathStack.Push(stackElement)
        End If

        nillableString = ""
        If runMode.showNillable And xsdElement.IsNillable Then
            nillableString = "{nillable}"
        End If
        occursString = ""

        If runMode.indicateRepeatble Then
            If xsdElement.MaxOccurs > 1 Then
                occursString = "[]"
            End If
        End If
        If runMode.showOccurs Then
            If xsdElement.MinOccurs <> 1 Or xsdElement.MaxOccurs <> 1 Then
                occursStringMin = xsdElement.MinOccurs
                If xsdElement.MaxOccurs > 1 Then
                    occursStringMax = xsdElement.MaxOccursString.Replace("unbounded", "n")
                Else
                    occursStringMax = "1"
                End If
                occursString = "[" & occursStringMin & ".." & occursStringMax & "]"
            End If
        End If

        If occursString.Length > 0 Or nillableString.Length > 0 Then
            currentXPath = currentXPath & "/" & xsdElement.Name & occursString & nillableString
        Else
            currentXPath = currentXPath & "/" & xsdElement.Name
        End If

        Select Case xsdElement.ElementSchemaType.GetType.Name
            Case "XmlSchemaComplexType"
                ' Get the complex type of the Customer element.
                Dim complexType As XmlSchemaComplexType = CType(xsdElement.ElementSchemaType, XmlSchemaComplexType)

                'Depending on the type of content
                Select Case complexType.ContentTypeParticle.GetType.Name
                    Case "XmlSchemaSequence"
                        ' Get the sequence particle of the complex type.
                        Dim sequence As XmlSchemaSequence = CType(complexType.ContentTypeParticle, XmlSchemaSequence)
                        processItems(sequence, currentXPath)
                    Case Else
                        Me._XPathErrors = Me._XPathErrors & vbCrLf & "ERROR: Unsupported ContentTypeParticle type - [" & complexType.ContentTypeParticle.GetType.Name & "]"
                End Select

            Case "XmlSchemaSimpleType"
                ' Get the simple type of the Customer element.
                Call simpleTypeAnalyzer.analyze(CType(xsdElement.ElementSchemaType, XmlSchemaSimpleType))
                sTemp = ""
                If runMode.showBaseTypeName Then
                    sTemp = simpleTypeAnalyzer.name
                End If
                If runMode.showLength Then
                    sTemp = sTemp & "," & simpleTypeAnalyzer.minMaxLegth
                End If
                If sTemp.Length > 0 Then
                    If sTemp.Substring(1, 1) <> "," Then
                        sTemp = "," & sTemp
                    End If
                End If
                _XPath = _XPath & currentXPath & sTemp & vbCrLf
        End Select

        XPathStack.Pop()

    End Sub

    Private Sub processItems(xsdItems As Object, currentXPath As String)
        For Each item In xsdItems.Items
            Select Case item.GetType.Name
                Case "XmlSchemaElement"
                    processElement(item, currentXPath)
                Case "XmlSchemaChoice"
                    processItems(item, currentXPath)
            End Select
        Next

    End Sub

    Private Function getTypeName(xsdElement As XmlSchemaElement) As String
        getTypeName = ""
        Select Case xsdElement.ElementSchemaType.GetType.Name
            Case "XmlSchemaComplexType"
                ' Get the complex type of the Customer element.
                Dim complexType As XmlSchemaComplexType = CType(xsdElement.ElementSchemaType, XmlSchemaComplexType)
                getTypeName = complexType.Name
            Case "XmlSchemaSimpleType"
                ' Get the complex type of the Customer element.
                Dim simpleType As XmlSchemaSimpleType = CType(xsdElement.ElementSchemaType, XmlSchemaSimpleType)
                getTypeName = simpleType.Name
            Case Else
                Me._XPathErrors = Me._XPathErrors & vbCrLf & "ERROR: Unsupported ElementSchemaType type - [" & xsdElement.ElementSchemaType.GetType.Name & "]"
        End Select
    End Function

    Shared Sub ValidationCallback(ByVal sender As Object, ByVal args As ValidationEventArgs)
        If args.Severity = XmlSeverityType.Warning Then
            Console.Write("WARNING: ")
        Else
            If args.Severity = XmlSeverityType.Error Then
                Console.Write("ERROR: ")
            End If
        End If
        Console.WriteLine(args.Message)
    End Sub

    Public ReadOnly Property GlobalElements() As ICollection
        Get
            If schemaSet Is Nothing Then
                Return Nothing
            Else
                Return schemaSet.GlobalElements.Names
            End If
        End Get
    End Property

    Public ReadOnly Property XPath() As String
        Get
            Return _XPath
        End Get
    End Property

    Public ReadOnly Property XPathErrors() As String
        Get
            Return _XPathErrors
        End Get
    End Property

End Class
