﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Generic
Imports System.Diagnostics
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic

    ''' <summary>
    ''' A region analysis walker that records jumps out of the region.
    ''' </summary>
    ''' <remarks></remarks>
    Class ExitPointsWalker
        Inherits AbstractRegionControlFlowPass

        Friend Overloads Shared Function Analyze(info As FlowAnalysisInfo, region As FlowAnalysisRegionInfo) As IEnumerable(Of StatementSyntax)
            Dim walker = New ExitPointsWalker(info, region)
            Try
                Return If(walker.Analyze(), walker.branchesOutOf.ToImmutable(), SpecializedCollections.EmptyEnumerable(Of StatementSyntax)())
            Finally
                walker.Free()
            End Try
        End Function

        Dim branchesOutOf As ArrayBuilder(Of StatementSyntax) = ArrayBuilder(Of StatementSyntax).GetInstance()

        Private Overloads Function Analyze() As Boolean
            '  only one pass is needed.
            Return Scan()
        End Function

        Private Sub New(info As FlowAnalysisInfo, region As FlowAnalysisRegionInfo)
            MyBase.New(info, region)
        End Sub

        Protected Overrides Sub Free()
            Me.branchesOutOf.Free()
            Me.branchesOutOf = Nothing
            Me.labelsInside.Free()
            Me.labelsInside = Nothing
            MyBase.Free()
        End Sub

        Dim labelsInside As ArrayBuilder(Of LabelSymbol) = ArrayBuilder(Of LabelSymbol).GetInstance()

        Public Overrides Function VisitLabelStatement(node As BoundLabelStatement) As BoundNode
            ' The syntax can be a label or an end block statement when the label represents an exit
            Dim syntax = node.Syntax
            If IsInside Then
                labelsInside.Add(node.Label)
            End If
            Return MyBase.VisitLabelStatement(node)
        End Function

        Public Overrides Function VisitDoLoopStatement(node As BoundDoLoopStatement) As BoundNode
            If IsInside Then
                labelsInside.Add(node.ExitLabel)
                labelsInside.Add(node.ContinueLabel)
            End If
            Return MyBase.VisitDoLoopStatement(node)
        End Function

        Public Overrides Function VisitForToStatement(node As BoundForToStatement) As BoundNode
            If IsInside Then
                labelsInside.Add(node.ExitLabel)
                labelsInside.Add(node.ContinueLabel)
            End If
            Return MyBase.VisitForToStatement(node)
        End Function

        Public Overrides Function VisitForEachStatement(node As BoundForEachStatement) As BoundNode
            If IsInside Then
                labelsInside.Add(node.ExitLabel)
                labelsInside.Add(node.ContinueLabel)
            End If
            Return MyBase.VisitForEachStatement(node)
        End Function

        Public Overrides Function VisitWhileStatement(node As BoundWhileStatement) As BoundNode
            If IsInside Then
                labelsInside.Add(node.ExitLabel)
                labelsInside.Add(node.ContinueLabel)
            End If
            Return MyBase.VisitWhileStatement(node)
        End Function

        Public Overrides Function VisitSelectStatement(node As BoundSelectStatement) As BoundNode
            If IsInside Then
                labelsInside.Add(node.ExitLabel)
            End If
            Return MyBase.VisitSelectStatement(node)
        End Function

        Protected Overrides Sub LeaveRegion()
            '  Process the pending returns only from this region. 
            For Each pending In Me.PendingBranches
                If IsInsideRegion(pending.Branch.Syntax.Span) Then
                    Select Case pending.Branch.Kind
                        Case BoundKind.GotoStatement
                            If labelsInside.Contains((TryCast((pending.Branch), BoundGotoStatement)).Label) Then
                                Continue For
                            End If
                        Case BoundKind.ExitStatement
                            If labelsInside.Contains((TryCast((pending.Branch), BoundExitStatement)).Label) Then
                                Continue For
                            End If
                        Case BoundKind.ContinueStatement
                            If labelsInside.Contains((TryCast((pending.Branch), BoundContinueStatement)).Label) Then
                                Continue For
                            End If
                        Case BoundKind.YieldStatement
                        Case BoundKind.ReturnStatement
                            ' These are always included (we don't dive into lambda expressions)
                        Case Else
                            Debug.Assert(False) ' there are no other branch statements
                    End Select
                    branchesOutOf.Add(DirectCast(pending.Branch.Syntax, StatementSyntax))
                End If
            Next

            MyBase.LeaveRegion()
        End Sub

    End Class

End Namespace
