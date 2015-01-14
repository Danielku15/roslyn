﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.


Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic

    ' Note: this code has a copy-and-paste sibling in AbstractRegionDataFlowPass.
    ' Any fix to one should be applied to the other.
    MustInherit Class AbstractRegionControlFlowPass
        Inherits ControlFlowPass

        Friend Sub New(info As FlowAnalysisInfo, region As FlowAnalysisRegionInfo)
            MyBase.New(info, region, False)
        End Sub

        Protected Overrides Sub Visit(node As BoundNode, Optional dontLeaveRegion As Boolean = False)
            ' Step into expressions as they may contain lambdas
            VisitAlways(node, dontLeaveRegion:=dontLeaveRegion)
        End Sub

        ' Control flow analysis does not normally scan the body of a lambda, but region analysis does.
        Public Overrides Function VisitLambda(node As BoundLambda) As BoundNode
            Dim oldPending As SavedPending = SavePending()
            Dim oldSymbol = Me.symbol
            Me.symbol = node.LambdaSymbol
            Dim finalState As LocalState = Me.State
            Me.SetState(Me.ReachableState())
            VisitBlock(node.Body)
            Me.symbol = oldSymbol
            RestorePending(oldPending)
            Me.IntersectWith(finalState, Me.State)
            Me.SetState(finalState)
            Return Nothing
        End Function

        Public Overrides Function VisitQueryLambda(node As BoundQueryLambda) As BoundNode
            Visit(node.Expression)
            Return Nothing
        End Function

        Public Overrides Function VisitQueryExpression(node As BoundQueryExpression) As BoundNode
            Visit(node.LastOperator)
            Return Nothing
        End Function

        Public Overrides Function VisitQuerySource(node As BoundQuerySource) As BoundNode
            Visit(node.Expression)
            Return Nothing
        End Function

        Public Overrides Function VisitQueryableSource(node As BoundQueryableSource) As BoundNode
            Visit(node.Source)
            Return Nothing
        End Function

        Public Overrides Function VisitToQueryableCollectionConversion(node As BoundToQueryableCollectionConversion) As BoundNode
            Visit(node.ConversionCall)
            Return Nothing
        End Function

        Public Overrides Function VisitQueryClause(node As BoundQueryClause) As BoundNode
            Visit(node.UnderlyingExpression)
            Return Nothing
        End Function

        Public Overrides Function VisitAggregateClause(node As BoundAggregateClause) As BoundNode
            If node.CapturedGroupOpt IsNot Nothing Then
                Visit(node.CapturedGroupOpt)
            End If

            Visit(node.UnderlyingExpression)
            Return Nothing
        End Function

        Public Overrides Function VisitOrdering(node As BoundOrdering) As BoundNode
            Visit(node.UnderlyingExpression)
            Return Nothing
        End Function

        Public Overrides Function VisitRangeVariableAssignment(node As BoundRangeVariableAssignment) As BoundNode
            Visit(node.Value)
            Return Nothing
        End Function

    End Class

End Namespace
