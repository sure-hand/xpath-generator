Imports System.Xml
Imports System.Xml.Schema

Public Class XMLSchemaSimpleTypeInfo
    Dim _name As XmlQualifiedName
    Dim _baseTypeName As XmlQualifiedName
    Public minLength As Integer
    Public maxLength As Integer

    Sub analyze(simpleType As XmlSchemaSimpleType)
        Dim restriction As XmlSchemaSimpleTypeRestriction = CType(simpleType.Content, XmlSchemaSimpleTypeRestriction)
        Dim aFacet As Object

        _name = simpleType.QualifiedName

        minLength = -1
        maxLength = -1
        _baseTypeName = restriction.BaseTypeName
        Select Case _baseTypeName.Name
            Case "string"
                For Each aFacet In restriction.Facets
                    Select Case aFacet.GetType.Name
                        Case "XmlSchemaMinLengthFacet"
                            minLength = aFacet.Value
                        Case "XmlSchemaMaxLengthFacet"
                            maxLength = aFacet.Value
                    End Select
                Next
        End Select

    End Sub

    Public ReadOnly Property minMaxLegth() As String
        Get
            minMaxLegth = ""
            Select Case _baseTypeName.Name
                Case "string"
                    If minLength > 0 Then
                        minMaxLegth = minLength & ","
                    Else
                        minMaxLegth = ","
                    End If
                    If maxLength > 0 Then
                        minMaxLegth = minMaxLegth & maxLength
                    End If
                Case Else
                    minMaxLegth = ","
            End Select
            Return minMaxLegth
        End Get
    End Property

    Public ReadOnly Property name() As String
        Get
            If _baseTypeName.Name = "anySimpleType" Then
                name = _name.Name
            Else
                name = _baseTypeName.Name
            End If
        End Get
    End Property
End Class
