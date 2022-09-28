Public Class ComputerPlayer
    Inherits Player

    Public Sub New(username As String, Board As List(Of BoardPosition), PlayerColour As ConsoleColor)
        MyBase.New(username, 0, 0, 0, Board, PlayerColour)
    End Sub


    Protected Overrides Function IsHuman() As Boolean
        Return False
    End Function

    Protected Overrides Function DecisionBuyProperty(Square As BoardProperty, Price As Integer) As Boolean

        If money < Price Then
            Return False
        End If

        Randomize()

        'if the price is different to usual price, make decision based on how good the value is
        Dim ChanceOfBuying As Single = 10
        If Price <> Square.PriceToBuy Then
            ChanceOfBuying *= Square.PriceToBuy / Price
        End If

        'increase chance of buying if player has more money
        ChanceOfBuying *= money / Price

        'increase chance of buyign if less properties available
        For Each prop In ListOfSellablePositions
            If prop.Owner IsNot Nothing Then
                ChanceOfBuying *= 1.1
            End If
        Next

        'increase probability if you have more money or if other properties have been bought

        If Square.GetType() = GetType(BoardProperty) Then

            'if already own one of that colour then higher chance of buying 
            For Each prop In ListOfBoardProperties
                If prop.Colour Is Square.Colour And prop.Owner Is Me Then
                    'multiply by larger number for brown and dark blue properties, as there are only 2 of that colour
                    If prop.Colour = "Brown" Or prop.Colour = "Dark Blue" Then
                        ChanceOfBuying *= 2.2
                    Else
                        ChanceOfBuying *= 1.5
                    End If
                End If
            Next

            Select Case Square.Colour
                Case "Brown"
                    ChanceOfBuying *= 1 / 4
                Case "Light Blue"
                    ChanceOfBuying *= 1 / 2
                Case "Pink"
                    'probability of buying pink property lower when only 2 players
                    If ListOfPlayers.Count = 2 Then
                        ChanceOfBuying *= 1 / 3
                    Else
                        'probability of buying pink property higher when more than 2 players
                        ChanceOfBuying *= 2 / 3
                    End If
                Case "Orange"
                    ChanceOfBuying *= 6 / 5
                Case "Red"
                    'probability of buying red property lower when only 2 players
                    If ListOfPlayers.Count = 2 Then
                        ChanceOfBuying *= 1 / 5
                    Else
                        'probability of buying red property higher when more than 2 players
                        ChanceOfBuying *= 6 / 5
                    End If
                Case "Yellow"
                    'probability of buying yellow property lower when only 2 players
                    If ListOfPlayers.Count = 2 Then
                        ChanceOfBuying *= 1 / 10
                    Else
                        'probability of buying yellow property = 9/10 when more than 2 players
                        ChanceOfBuying *= 6 / 5
                    End If
                Case "Green"
                    'probability of buying green property lower when only 2 players
                    If ListOfPlayers.Count = 2 Then
                        ChanceOfBuying *= 1 / 15
                    ElseIf ListOfPlayers.Count = 3 Then
                        'probability of buying green property higher when 3 players
                        ChanceOfBuying *= 1 / 2
                    Else
                        'probability of buying green property even higher when more than 3 players
                        ChanceOfBuying *= 7 / 10
                    End If
                Case "Dark Blue"
                    'probability of buying dark blue property lower when only 2 players
                    If ListOfPlayers.Count = 2 Then
                        ChanceOfBuying *= 1 / 10
                    Else
                        'probability of buying dark blue property higher when more than 2 players
                        ChanceOfBuying *= 1 / 2
                    End If
            End Select

        ElseIf Square.GetType() = GetType(BoardStation) Then
            'the more stations owned, the higher the chance of buying
            For Each prop In ListOfBoardStations
                If prop.Owner Is Me Then
                    ChanceOfBuying *= 1.2
                End If
            Next
            'probability of buying a station lower when 2 players
            If ListOfPlayers.Count = 2 Then
                ChanceOfBuying *= 3 / 10
            Else
                'probability of buying station higher when more than 2 players
                ChanceOfBuying *= 1 / 2
            End If

        ElseIf Square.GetType() = GetType(BoardUtility) Then
            'if already own one utility then increase chance of buying
            For Each prop In ListOfBoardUtilities
                If prop.Owner Is Me Then
                    ChanceOfBuying *= 2.2
                End If
            Next
            'probability of buying a utility lower when 2 players
            If ListOfPlayers.Count = 2 Then
                ChanceOfBuying *= 3 / 10
            Else
                'probability of buying station higher when more than 2 players
                ChanceOfBuying *= 1 / 2

            End If
        End If

        Return ((100 * Rnd()) + 1) < ChanceOfBuying

    End Function

    Protected Overrides Function DecisionBuyStation(Square As BoardStation) As Boolean

        If money > Square.PriceToBuy * 2 Then
            Return True
        End If
        Return False

    End Function

    Protected Overrides Function DecisionBuyUtility(Square As BoardUtility) As Boolean

        If money > Square.PriceToBuy * 2 Then
            Return True
        End If
        Return False

    End Function

    Protected Overrides Function DecisionUseGetOutOfJailFreeCard(NumberOfGetOutOfJailCards As Integer) As Boolean

        Dim NumberOfPropertiesAvailable As Integer = 0

        For Each prop In ListOfSellablePositions
            If prop.Owner Is Nothing Then
                NumberOfPropertiesAvailable += 1
            End If
        Next

        'better to stay in jail if lots of properties have been bought
        Return NumberOfPropertiesAvailable > 22

    End Function

    Protected Overrides Function DecisionPayToGetOutOfJail() As Boolean

        If money < 100 Then
            Return False
        Else
            Dim NumberOfPropertiesAvailable As Integer = 0

            For Each prop In ListOfSellablePositions
                If prop.Owner Is Nothing Then
                    NumberOfPropertiesAvailable += 1
                End If
            Next

            'better to stay in jail if lots of properties have been bought
            Return NumberOfPropertiesAvailable > 8
        End If

    End Function

    Protected Overrides Sub DecisionEndTurn()

        DecisionBuyBuildings()
        'Sell(ListOfPlayers)
        'Mortgage()
        'SeeStatusOfProperties() <-- for testing

    End Sub

    Protected Overrides Sub DecisionBuyBuildings()

        Dim HouseBuildColours As New HashSet(Of String)
        Dim HotelBuildColours As New HashSet(Of String)
        Dim CanBuildHouse As Boolean = False
        Dim CanBuildHotel As Boolean = False
        Dim HouseBuildList As New List(Of BoardProperty)
        Dim HotelBuildList As New List(Of BoardProperty)

        GetHouseHotelBuildLists(HouseBuildList, HotelBuildList, HouseBuildColours, HotelBuildColours)

        CanBuildHouse = HouseBuildList.Count > 0
        CanBuildHotel = HotelBuildList.Count > 0

        If CanBuildHouse = True Or CanBuildHotel = True Then
            If CanBuildHouse Then
                DecisionHouseBuy(HouseBuildList)
            End If
            If CanBuildHotel Then
                DecisionHotelBuy(HotelBuildList)
            End If
        End If

    End Sub

    Sub DecisionHouseBuy(HouseBuildList)


        For Each Prop As BoardProperty In HouseBuildList
            If Prop.BuildingCost < money Then
                HouseBuy(Prop)
            End If
        Next

    End Sub

    Sub DecisionHotelBuy(HotelBuildList)

        For Each Prop As BoardProperty In HotelBuildList
            If Prop.BuildingCost < money Then
                HotelBuy(Prop)
            End If
        Next

    End Sub

    Protected Overrides Function DecisionSellBuildings() As Boolean

        Dim ListOfProperties As New List(Of BoardProperty)

        For Each Prop As BoardProperty In ListOfBoardProperties
            If Prop.Owner Is Me Then
                If Prop.NumberOfHotels > 0 Then
                    SellHotel(Prop)
                    Return True
                End If
                If Prop.NumberOfHouses > 0 Then
                    SellHouse(Prop)
                    Return True
                End If
            End If
        Next

        Return False

    End Function

    Protected Overrides Sub DecisionSell(ListOfPlayers As List(Of Player), MoneyRequired As Integer)


        Do Until money >= MoneyRequired

            If DecisionSellBuildings() Then
                Continue Do
            End If
            'finish if theres time
            'If SellProperties(ListOfPlayers) Then
            '    Continue Do
            'End If
            Return
        Loop

    End Sub

End Class
