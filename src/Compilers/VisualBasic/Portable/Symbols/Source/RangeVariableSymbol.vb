﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Generic
Imports System.Collections.Immutable
Imports System.Collections.ObjectModel
Imports System.Threading
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic.Symbols
    ''' <summary>
    ''' Represents a range variable symbol.
    ''' </summary>
    Friend MustInherit Class RangeVariableSymbol
        Inherits Symbol
        Implements IRangeVariableSymbol

        Friend ReadOnly m_Binder As Binder
        Private ReadOnly m_Type As TypeSymbol

        Public MustOverride ReadOnly Property Syntax As VisualBasicSyntaxNode

        Private Sub New(
            binder As Binder,
            type As TypeSymbol
        )
            m_Binder = binder
            m_Type = type
        End Sub

        Public Overrides ReadOnly Property Kind As SymbolKind
            Get
                Return SymbolKind.RangeVariable
            End Get
        End Property

        Public Overrides ReadOnly Property ContainingSymbol As Symbol
            Get
                Return m_Binder.ContainingMember
            End Get
        End Property

        Public Overridable ReadOnly Property Type As TypeSymbol
            Get
                Return m_Type
            End Get
        End Property

        Public MustOverride Overrides ReadOnly Property Name As String
        Public MustOverride Overrides ReadOnly Property Locations As ImmutableArray(Of Location)

        Public Overrides ReadOnly Property DeclaredAccessibility As Accessibility
            Get
                Return Accessibility.NotApplicable
            End Get
        End Property

        Public Overrides ReadOnly Property IsShared As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property IsOverridable As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property IsMustOverride As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property IsNotOverridable As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property IsOverrides As Boolean
            Get
                Return False
            End Get
        End Property

        Friend NotOverridable Overrides ReadOnly Property ObsoleteAttributeData As ObsoleteAttributeData
            Get
                Return Nothing
            End Get
        End Property

        Friend Overrides Function Accept(Of TArgument, TResult)(visitor As VisualBasicSymbolVisitor(Of TArgument, TResult), arg As TArgument) As TResult
            Return visitor.VisitRangeVariable(Me, arg)
        End Function

#Region "ISymbol"

        Public Overrides Sub Accept(visitor As SymbolVisitor)
            visitor.VisitRangeVariable(DirectCast(Me, IRangeVariableSymbol))
        End Sub

        Public Overrides Function Accept(Of TResult)(visitor As SymbolVisitor(Of TResult)) As TResult
            Return visitor.VisitRangeVariable(DirectCast(Me, IRangeVariableSymbol))
        End Function

        Public Overrides Sub Accept(visitor As VisualBasicSymbolVisitor)
            visitor.VisitRangeVariable(Me)
        End Sub

        Public Overrides Function Accept(Of TResult)(visitor As VisualBasicSymbolVisitor(Of TResult)) As TResult
            Return visitor.VisitRangeVariable(Me)
        End Function

#End Region

        ''' <summary>
        ''' Create a range variable symbol associated with an identifier token.
        ''' </summary>
        Friend Shared Function Create(
            binder As Binder,
            declaringIdentifier As SyntaxToken,
            type As TypeSymbol
        ) As RangeVariableSymbol
            Return New WithIdentifierToken(binder, declaringIdentifier, type)
        End Function

        ''' <summary>
        ''' Create a range variable symbol not associated with an identifier token, i.e. with illegal name.
        ''' Used for error recovery binding.
        ''' </summary>
        Friend Shared Function CreateForErrorRecovery(
            binder As Binder,
            syntax As VisualBasicSyntaxNode,
            type As TypeSymbol
        ) As RangeVariableSymbol
            Return New ForErrorRecovery(binder, syntax, type)
        End Function

        Friend Shared Function CreateCompilerGenerated(
            binder As Binder,
            syntax As VisualBasicSyntaxNode,
            name As String,
            type As TypeSymbol
        ) As RangeVariableSymbol
            Return New CompilerGenerated(binder, syntax, name, type)
        End Function

        Private Class WithIdentifierToken
            Inherits RangeVariableSymbol

            Private ReadOnly m_IdentifierToken As SyntaxToken

            Public Sub New(
                binder As Binder,
                declaringIdentifier As SyntaxToken,
                type As TypeSymbol
            )
                MyBase.New(binder, type)
                m_IdentifierToken = declaringIdentifier
            End Sub

            Public Overrides ReadOnly Property Name As String
                Get
                    Return m_IdentifierToken.GetIdentifierText()
                End Get
            End Property

            Public Overrides ReadOnly Property Syntax As VisualBasicSyntaxNode
                Get
                    Return DirectCast(m_IdentifierToken.Parent, VisualBasicSyntaxNode)
                End Get
            End Property

            Public Overrides ReadOnly Property Locations As ImmutableArray(Of Location)
                Get
                    Return ImmutableArray.Create(Of Location)(m_IdentifierToken.GetLocation())
                End Get
            End Property

            Public Overrides ReadOnly Property DeclaringSyntaxReferences As ImmutableArray(Of SyntaxReference)
                Get
                    Dim parent As VisualBasicSyntaxNode
                    Dim grandParent As VisualBasicSyntaxNode = Nothing
                    Dim ggParent As VisualBasicSyntaxNode = Nothing
                    parent = DirectCast(m_IdentifierToken.Parent, VisualBasicSyntaxNode)
                    If parent IsNot Nothing Then
                        grandParent = parent.Parent
                    End If
                    If grandParent IsNot Nothing Then
                        ggParent = grandParent.Parent
                    End If

                    Dim collectionRange = TryCast(grandParent, CollectionRangeVariableSyntax)
                    If collectionRange IsNot Nothing AndAlso m_IdentifierToken = collectionRange.Identifier.Identifier Then
                        Return ImmutableArray.Create(Of SyntaxReference)(collectionRange.GetReference())
                    End If

                    Dim expressionRange = TryCast(ggParent, ExpressionRangeVariableSyntax)
                    If expressionRange IsNot Nothing AndAlso expressionRange.NameEquals IsNot Nothing AndAlso expressionRange.NameEquals.Identifier.Identifier = m_IdentifierToken Then
                        Return ImmutableArray.Create(Of SyntaxReference)(expressionRange.GetReference())
                    End If

                    Dim aggregationRange = TryCast(ggParent, AggregationRangeVariableSyntax)
                    If aggregationRange IsNot Nothing AndAlso aggregationRange.NameEquals IsNot Nothing AndAlso aggregationRange.NameEquals.Identifier.Identifier = m_IdentifierToken Then
                        Return ImmutableArray.Create(Of SyntaxReference)(aggregationRange.GetReference())
                    End If

                    Return ImmutableArray(Of SyntaxReference).Empty
                End Get
            End Property

            Public Overrides Function Equals(obj As Object) As Boolean
                'PERF: TryCast is to avoid going through GetObjectValue when calling ReferenceEquals
                Dim other = TryCast(obj, RangeVariableSymbol.WithIdentifierToken)

                If Me Is other Then
                    Return True
                End If

                Return other IsNot Nothing AndAlso other.m_IdentifierToken.Equals(m_IdentifierToken)
            End Function

            Public Overrides Function GetHashCode() As Integer
                Return m_IdentifierToken.GetHashCode()
            End Function

        End Class

        Private Class ForErrorRecovery
            Inherits RangeVariableSymbol

            Private ReadOnly m_Syntax As VisualBasicSyntaxNode

            Public Sub New(
                binder As Binder,
                syntax As VisualBasicSyntaxNode,
                type As TypeSymbol
            )
                MyBase.New(binder, type)
                Debug.Assert(syntax IsNot Nothing)
                m_Syntax = syntax
            End Sub

            Public Overrides ReadOnly Property Syntax As VisualBasicSyntaxNode
                Get
                    Return m_Syntax
                End Get
            End Property

            Public Overrides ReadOnly Property Name As String
                Get
                    Return "$"c & m_Syntax.Position.ToString()
                End Get
            End Property

            Public Overrides ReadOnly Property Locations As ImmutableArray(Of Location)
                Get
                    Return ImmutableArray.Create(Of Location)(m_Syntax.GetLocation())
                End Get
            End Property

            Public Overrides ReadOnly Property DeclaringSyntaxReferences As ImmutableArray(Of SyntaxReference)
                Get
                    Return ImmutableArray(Of SyntaxReference).Empty
                End Get
            End Property
        End Class

        Private Class CompilerGenerated
            Inherits ForErrorRecovery

            Private ReadOnly m_Name As String

            Public Sub New(
                binder As Binder,
                syntax As VisualBasicSyntaxNode,
                name As String,
                type As TypeSymbol
            )
                MyBase.New(binder, syntax, type)
                Debug.Assert(name IsNot Nothing)
                m_Name = name
            End Sub

            Public Overrides ReadOnly Property Name As String
                Get
                    Return m_Name
                End Get
            End Property
        End Class

    End Class

End Namespace
