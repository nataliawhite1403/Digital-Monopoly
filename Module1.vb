Imports System.IO
Imports System.Data.OleDb
Imports System.Reflection

Module Module1


    Public projectLocation As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    Public databaseLocation As String = Path.Combine(projectLocation, "LoginDatabase.ACCDB")
    Public databaseLocation2 As String = Path.Combine(projectLocation, "MonopolyProperties.ACCDB")

    Public connString As String = "Provider=Microsoft.Ace.OLEDB.12.0; Data Source=" & databaseLocation & ";"
    Public connString2 As String = "Provider=Microsoft.Ace.OLEDB.12.0; Data Source=" & databaseLocation2 & ";"


    Public ListOfPlayers As New List(Of Player)
    Public ListOfBoardPositions As New List(Of BoardPosition)

    Public ListOfSellablePositions As New List(Of SellablePosition)
    Public ListOfBoardProperties As New List(Of BoardProperty)
    Public ListOfBoardStations As New List(Of SellablePosition)
    Public ListOfBoardUtilities As New List(Of SellablePosition)

    Dim MainPlayer As Player
    Dim ColourOptions As New Queue(Of ConsoleColor)

    Sub Main()

        Console.SetWindowSize(180, 50)
        Console.SetWindowPosition(0, 0)

        Console.WriteLine("Enter 1 if using laptop")
        If Console.ReadLine() = "1" Then
            connString = "Provider=Microsoft.Ace.OLEDB.12.0; Data Source= C:\LoginDatabase.ACCDB"
            connString2 = "Provider=Microsoft.Ace.OLEDB.12.0; Data Source= C:\MonopolyProperties.ACCDB"
        End If

        Do

            Dim input As String

            ColourOptions.Enqueue(ConsoleColor.Red)
            ColourOptions.Enqueue(ConsoleColor.DarkGreen)
            ColourOptions.Enqueue(ConsoleColor.DarkBlue)
            ColourOptions.Enqueue(ConsoleColor.DarkYellow)
            ColourOptions.Enqueue(ConsoleColor.Magenta)
            ColourOptions.Enqueue(ConsoleColor.DarkCyan)

            While ListOfPlayers.Count = 0
                Console.WriteLine("Press 1 to login or anything else to create an account.")
                input = Console.ReadLine()
                If input = "1" Then
                    Dim player As Player = Login(ColourOptions)
                    If Not IsNothing(player) Then
                        ListOfPlayers.Add(player)
                        MainPlayer = player
                    End If
                Else
                    CreateAccount()
                End If
            End While

            MainMenu()

        Loop

    End Sub

    Function Login(ColourOptions) As Player

        Dim username, password As String
        Console.WriteLine("Enter username")
        username = Console.ReadLine()
        Console.WriteLine("Enter password")
        password = Console.ReadLine()

        For i = 0 To ListOfPlayers.Count - 1
            If username = ListOfPlayers(i).username Then
                Console.WriteLine("This player is already in the game.")
                Console.WriteLine()
                Return Nothing
            End If
        Next

        Dim searchString As String = "Select * From Logins Where [Username] = @username and [Password] = @password"

        Try
            'assign the database we established at the top of the code to the connection string element of conn
            Dim conn As New OleDbConnection
            conn.ConnectionString = connString
            'Open connection to the database
            conn.Open()

            'Cmd will represent the connection and sql statement that we wish to run on the database 
            Dim cmd As New OleDbCommand(searchString, conn)
            Dim dap As New OleDbDataAdapter
            dap.SelectCommand = cmd

            cmd.Parameters.AddWithValue("@Username", username)
            cmd.Parameters.AddWithValue("@Password", HashPassword(password).ToString)

            'using the data adapter, run the query and fill the datatable with the returned data results
            Dim dt As New DataTable
            dap.Fill(dt)

            'close the connection to the database
            conn.Close()

            If dt.Rows.Count = 0 Then
                Console.WriteLine("Username Or password incorrect.")
                Console.ReadLine()
            Else
                Dim row As DataRow = dt.Rows(0)
                Dim wins As Integer = row("Wins")
                Dim losses As Integer = row("Losses")
                Dim highscore As Integer = row("HighScore")
                Dim PlayerColour As ConsoleColor = ColourOptions.Dequeue()
                Return New HumanPlayer(username, wins, losses, highscore, ListOfBoardPositions, PlayerColour)
            End If

            'catch any errors in the process and output them if necessary
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Return Nothing

    End Function

    Sub CreateAccount()

        Dim username, password, PasswordCheck As String
        Dim taken As Boolean = True
        Dim match As Boolean = False
        Dim PasswordLength As Boolean = False
        Dim UsernameLength As Boolean = False
        Dim ExitSub As Boolean = False

        Do While (taken = True Or UsernameLength = False) And ExitSub = False
            Console.WriteLine("Create a username or enter 5 to exit.")
            username = Console.ReadLine()
            taken = False
            If username = "5" Then
                ExitSub = True
            End If

            If ExitSub = False Then
                Dim searchString As String = "Select Username From Logins"

                Dim conn As New OleDbConnection
                Dim dap As New OleDbDataAdapter
                Dim dt As New DataTable

                Try
                    'assign the database we established at the top of the code to the connection string element of conn           
                    conn.ConnectionString = connString
                    'Open connection to the database
                    conn.Open()

                    'Cmd will represent the connection and sql statement that we wish to run on the database 
                    Dim cmd As New OleDbCommand(searchString, conn)

                    dap.SelectCommand = cmd

                    'using the data adapter, run the query and fill the datatable with the returned data results

                    dap.Fill(dt)

                    'close the connection to the database
                    conn.Close()

                    'looping through each row of results in the datatable we have just populated
                    For Each row As DataRow In dt.Rows
                        If row("Username") = username Then
                            taken = True
                            Exit For
                        Else
                        End If
                    Next

                    For i = 1 To 5
                        If username = ("ComputerPlayer" & i) Then
                            taken = True
                            Exit For
                        End If
                    Next

                    If taken = True Then
                        Console.WriteLine("This username Is already taken. Try again.")
                    ElseIf username.Length < 5 Then
                        Console.WriteLine("Username Is too Short. It must be a minimum Of 5 characters.")
                    Else
                        UsernameLength = True
                        taken = False
                        Do Until match = True And PasswordLength = True
                            Console.WriteLine("Create a password")
                            password = Console.ReadLine()
                            If password.Length < 5 Then
                                Console.WriteLine("Password Is too Short. It must be a minimum Of 5 characters.")
                            Else
                                PasswordLength = True
                                Console.WriteLine("Confirm password")
                                PasswordCheck = Console.ReadLine()
                                If password <> PasswordCheck Then
                                    Console.WriteLine("The passwords Do Not match. Please Try again.")
                                Else
                                    match = True
                                    WriteAccount(username, password, conn, dap, dt, searchString)
                                    Console.WriteLine("Your account has been created.")
                                End If

                            End If
                        Loop
                    End If

                    'catch any errors in the process and output them if necessary
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    Return
                End Try
            End If
        Loop

    End Sub

    Sub WriteAccount(username, password, conn, dap, dt, searchString)

        Try
            'assign the database we established at the top of the code to the connection string element of conn
            conn.ConnectionString = connString
            'Open connection to the database
            conn.Open()

            'Cmd will represent the connection and sql statement that we wish to run on the database 
            Dim cmd As New OleDbCommand(searchString, conn)

            dap.SelectCommand = cmd

            'using the data adapter, run the query and fill the datatable with the returned data results

            dap.Fill(dt)

            Dim query As String = "INSERT INTO Logins ( [Username], [Password]) VALUES (@username, @password) "

            'Cmd will represent the connection and sql statement that we wish to run on the database

            Dim cmd2 As New OleDbCommand(query, conn)

            Dim HashValue As Integer = HashPassword(password)

            cmd2.Parameters.AddWithValue("@Username", username)
            cmd2.Parameters.AddWithValue("@Password", HashValue.ToString)


            'runs the command to upload details to the database table, this is different to a select command as we don’t expect a response

            cmd2.ExecuteNonQuery()

            'close the connection to the database
            conn.Close()

            'catch any errors in the process and output them if necessary
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Return
        End Try

    End Sub

    Public Function HashPassword(password As Object) As Integer

        Dim HashValue As Integer = 0
        For i = 0 To password.length - 1
            HashValue += Asc(password(i)) * (i + 1)
            HashValue = HashValue Mod Integer.MaxValue / 2
        Next

        Return HashValue
    End Function

    Sub Game()

        CreateBoard()

        'recreate the main player, this ensures that if the player plays more than one game, there is no leftover player data from the previous game
        If ListOfPlayers.Contains(MainPlayer) Then
            ListOfPlayers.Remove(MainPlayer)
        End If
        MainPlayer = New HumanPlayer(MainPlayer.username, MainPlayer.wins, MainPlayer.losses, MainPlayer.highscore, ListOfBoardPositions, MainPlayer.PlayerColour)
        ListOfPlayers.Add(MainPlayer)

        Player.NumberOfHousesAvailable = 32
        Player.NumberOfHotelsAvailable = 12

        SetUpChanceAndCommunityChestCards()

        Dim GameOver As Boolean = False

        Do Until GameOver = True
            Dim PlayersToTakeTurn As New List(Of Player)
            For Each player In ListOfPlayers
                PlayersToTakeTurn.Add(player)
            Next
            While PlayersToTakeTurn.Count > 0
                Dim CurrentPlayer As Player = PlayersToTakeTurn(0)
                PlayersToTakeTurn.RemoveAt(0)
                CurrentPlayer.TakeTurn(False, ListOfPlayers)
                'removes any players that have left
                For Each player In ListOfPlayers
                    If player.HasLeft = True Then
                        UpdateLosses(player)
                        RemovePlayer(player)
                    End If
                Next
                ListOfPlayers.RemoveAll(AddressOf HasPlayerLeft)
                PlayersToTakeTurn.RemoveAll(AddressOf HasPlayerLeft)

                If ListOfPlayers.Count = 1 Then
                    GameOver = True
                    Exit While
                End If
            End While
        Loop

        Dim TotalValue As Integer = CalculateTotalValue(TotalValue, ListOfPlayers)

        Console.WriteLine()
        Console.WriteLine("The winner is " & ListOfPlayers(0).username & " with £" & ListOfPlayers(0).money & " and a total value of £" & TotalValue)
        UpdatePlayerStats(TotalValue, ListOfPlayers)
        If TotalValue > ListOfPlayers(0).highscore Then
            Console.WriteLine("Congratulations " & ListOfPlayers(0).username & ", you have a new high score!")
        Else
            Console.WriteLine(ListOfPlayers(0).username & "'s high score is £" & ListOfPlayers(0).highscore)
        End If
        ListOfPlayers.Clear()
        Console.ReadLine()

    End Sub

    Private Sub SetUpChanceAndCommunityChestCards()
        Player.ChanceCards = New Queue(Of Integer)
        Player.CommunityChestCards = New Queue(Of Integer)

        Dim ListOfNumbers As New List(Of Integer)
        Dim RandomGenerator As New System.Random

        For i = 0 To 15
            ListOfNumbers.Add(i)
        Next

        While ListOfNumbers.Count > 0
            Dim Random As Integer = RandomGenerator.Next(0, ListOfNumbers.Count - 1)
            Player.ChanceCards.Enqueue(ListOfNumbers(Random))
            ListOfNumbers.RemoveAt(Random)
        End While


        For i = 0 To 15
            ListOfNumbers.Add(i)
        Next

        While ListOfNumbers.Count > 0
            Dim Random As Integer = RandomGenerator.Next(0, ListOfNumbers.Count - 1)
            Player.CommunityChestCards.Enqueue(ListOfNumbers(Random))
            ListOfNumbers.RemoveAt(Random)
        End While
    End Sub

    Sub UpdateLosses(player)

        Dim searchString As String = "Select * From Logins"

        Try
            'assign the database we established at the top of the code to the connection string element of conn
            Dim conn As New OleDbConnection
            conn.ConnectionString = connString
            'Open connection to the database
            conn.Open()

            'Cmd will represent the connection and sql statement that we wish to run on the database 
            Dim cmd As New OleDbCommand(searchString, conn)
            Dim dap As New OleDbDataAdapter
            dap.SelectCommand = cmd

            'using the data adapter, run the query and fill the datatable with the returned data results
            Dim dt As New DataTable
            dap.Fill(dt)

            Dim query As String = "UPDATE Logins SET  [Losses]  = @losses WHERE [Username] = @username "

            'Cmd will represent the connection and sql statement that we wish to run on the database

            Dim cmd2 As New OleDbCommand(query, conn)

            cmd2.Parameters.AddWithValue("@Losses", player.losses + 1)
            cmd2.Parameters.AddWithValue("@Username", player.username)

            'runs the command to upload details to the database table, this is different to a select command as we don’t expect a response

            cmd2.ExecuteNonQuery()

            'close the connection to the database
            conn.Close()

            'catch any errors in the process and output them if necessary
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Return
        End Try

    End Sub

    Sub UpdatePlayerStats(TotalValue, ListOfPlayers)

        Dim searchString As String = "Select * From Logins"

        Try
            'assign the database we established at the top of the code to the connection string element of conn
            Dim conn As New OleDbConnection
            conn.ConnectionString = connString
            'Open connection to the database
            conn.Open()

            'Cmd will represent the connection and sql statement that we wish to run on the database 
            Dim cmd As New OleDbCommand(searchString, conn)
            Dim dap As New OleDbDataAdapter
            dap.SelectCommand = cmd

            'using the data adapter, run the query and fill the datatable with the returned data results
            Dim dt As New DataTable
            dap.Fill(dt)

            If TotalValue > ListOfPlayers(0).highscore Then

                Dim query As String = "UPDATE Logins SET  [HighScore]  = @TotalValue WHERE [Username] = @username "

                'Cmd will represent the connection and sql statement that we wish to run on the database

                Dim cmd2 As New OleDbCommand(query, conn)

                cmd2.Parameters.AddWithValue("@HighScore", TotalValue)
                cmd2.Parameters.AddWithValue("@Username", ListOfPlayers(0).username)

                'runs the command to upload details to the database table, this is different to a select command as we don’t expect a response

                cmd2.ExecuteNonQuery()

            End If

            Dim query2 As String = "UPDATE Logins SET  [Wins]  = @Wins WHERE [Username] = @username "

            'Cmd will represent the connection and sql statement that we wish to run on the database

            Dim cmd3 As New OleDbCommand(query2, conn)

            cmd3.Parameters.AddWithValue("@Wins", ListOfPlayers(0).Wins + 1)
            cmd3.Parameters.AddWithValue("@Username", ListOfPlayers(0).username)

            'runs the command to upload details to the database table, this is different to a select command as we don’t expect a response

            cmd3.ExecuteNonQuery()

            'close the connection to the database
            conn.Close()

            'catch any errors in the process and output them if necessary
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Return
        End Try

    End Sub

    Function CalculateTotalValue(TotalValue, ListOfPlayers) As Integer

        For Each Prop As BoardProperty In ListOfBoardProperties
            If Prop.Owner Is ListOfPlayers(0) Then
                TotalValue += (Prop.NumberOfHotels * (Prop.BuildingCost / 2) * 5)
                TotalValue += (Prop.NumberOfHouses * (Prop.BuildingCost / 2))
            End If
        Next

        For Each Square As SellablePosition In ListOfSellablePositions
            If Square.Owner Is ListOfPlayers(0) Then
                TotalValue += Square.PriceToBuy
                If Square.IsMortgaged = True Then
                    TotalValue -= (Square.MortgageValue * 1.1)
                End If
            End If
        Next

        TotalValue += ListOfPlayers(0).money

        Return TotalValue

    End Function

    Sub RemovePlayer(CurrentPlayer)

        Console.WriteLine()
        If CurrentPlayer.IsBankrupt < 0 Then
            Console.BackgroundColor = CurrentPlayer.PlayerColour
            Console.Write(CurrentPlayer.username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.WriteLine(" has lost the game.")
        Else
            Console.BackgroundColor = CurrentPlayer.PlayerColour
            Console.Write(CurrentPlayer.username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.WriteLine(" has left the game.")
        End If

        For Each Prop As BoardProperty In ListOfBoardProperties
            If Prop.Owner Is CurrentPlayer Then
                Prop.NumberOfHotels = 0
                Prop.NumberOfHouses = 0
            End If
        Next

        For Each Position As SellablePosition In ListOfSellablePositions
            If Position.Owner Is CurrentPlayer Then
                Position.Owner = Nothing
            End If
        Next

        If CurrentPlayer.DoesPlayerHaveChanceGetOutOfJailCard = True Then
            CurrentPlayer.DoesPlayerHaveChanceGetOutOfJailCard = False
            Player.ChanceCards.Enqueue(0)
        End If
        If CurrentPlayer.DoesPlayerHaveCommunityChestGetOutOfJailCard = True Then
            CurrentPlayer.DoesPlayerHaveCommunityChestGetOutOfJailCard = False
            Player.CommunityChestCards.Enqueue(5)
        End If

    End Sub

    Sub CreateBoard()

        ListOfBoardPositions.Clear()
        ListOfSellablePositions.Clear()
        ListOfBoardProperties.Clear()
        ListOfBoardStations.Clear()
        ListOfBoardUtilities.Clear()

        Dim searchString As String = "Select * From [Position] Order By [ID] ASC"

        ' Try
        'assign the database we established at the top of the code to the connection string element of conn
        Dim conn As New OleDbConnection
            conn.ConnectionString = connString2
            'Open connection to the database
            conn.Open()

            'Cmd will represent the connection and sql statement that we wish to run on the database 
            Dim cmd As New OleDbCommand(searchString, conn)
            Dim dap As New OleDbDataAdapter
            dap.SelectCommand = cmd

            'using the data adapter, run the query and fill the datatable with the returned data results
            Dim dt As New DataTable
            dap.Fill(dt)

            'close the connection to the database
            conn.Close()

            'looping through each row of results in the datatable we have just populated
            For Each row As DataRow In dt.Rows
                Dim ID As String = row("ID")
                Dim Name As String = row("Name")
                Dim Type As String = row("Type")
            Dim Row1Text As String = ""
            Dim Row2Text As String = ""
            If row("Row1Text") IsNot DBNull.Value Then
                Row1Text = row("Row1Text")
            End If
            If row("Row2Text") IsNot DBNull.Value Then
                Row2Text = row("Row2Text")
            End If
            Select Case Type
                    Case "Go", "Jail", "Parking"
                        ListOfBoardPositions.Add(New BoardOther(ID, Name, Row1Text, Row2Text))
                    Case "Property"
                        ListOfBoardPositions.Add(New BoardProperty(ID, Name, Row1Text, Row2Text))
                    Case "Station"
                        ListOfBoardPositions.Add(New BoardStation(ID, Name, Row1Text, Row2Text))
                    Case "Utility"
                        ListOfBoardPositions.Add(New BoardUtility(ID, Name, Row1Text, Row2Text))
                    Case "Chance"
                        ListOfBoardPositions.Add(New BoardChance(ID, Name, Row1Text, Row2Text))
                    Case "Chest"
                        ListOfBoardPositions.Add(New BoardChest(ID, Name, Row1Text, Row2Text))
                    Case "GoToJail"
                        ListOfBoardPositions.Add(New BoardGoToJail(ID, Name, Row1Text, Row2Text))
                    Case "Tax"
                        ListOfBoardPositions.Add(New BoardTax(ID, Name, Row1Text, Row2Text))
                End Select
            Next

            dt.Clear()

        'catch any errors in the process and output them if necessary
        'Catch ex As Exception
        '    Console.WriteLine(ex.Message)
        ' End Try

        For Each Square In ListOfBoardPositions
            If Square.GetType() = GetType(BoardProperty) Then
                ListOfBoardProperties.Add(Square)
                ListOfSellablePositions.Add(Square)
            ElseIf Square.GetType() = GetType(BoardStation) Then
                ListOfBoardStations.Add(Square)
                ListOfSellablePositions.Add(Square)
            ElseIf Square.GetType() = GetType(BoardUtility) Then
                ListOfBoardUtilities.Add(Square)
                ListOfSellablePositions.Add(Square)
            End If
        Next

    End Sub

    Sub MainMenu()

        Dim choice As String

        Do While choice <> "5"

            Console.WriteLine()
            Console.WriteLine("Username: " & ListOfPlayers(0).username & "       Wins: " & ListOfPlayers(0).wins & "      Losses: " & ListOfPlayers(0).losses & "      High Score: £" & ListOfPlayers(0).highscore)
            Console.WriteLine()
            Console.WriteLine("1. Play game")
            Console.WriteLine("2. View leaderboard")
            Console.WriteLine("3. View instructions")
            Console.WriteLine("4. Account management")
            Console.WriteLine("5. Sign out")

            Console.WriteLine("What would you like to do? (enter number)")
            choice = Console.ReadLine()
            Select Case choice
                Case "1"
                    PlayGame()
                Case "2"
                    Leaderboard()
                Case "3"
                    Instructions()
                Case "4"
                    ListOfPlayers(0).AccountManagement()
                Case "5"
                    ListOfPlayers.Clear()
                Case Else
                    Console.WriteLine("Input invalid.")
            End Select
        Loop

    End Sub

    Sub PlayGame()

        Dim choice1, choice2 As String
        Dim valid As Boolean = False

        Console.WriteLine()
        Console.WriteLine("1. Play online with friends")
        Console.WriteLine("2. Play online with random opponents")
        Console.WriteLine("3. Play offline with friends")
        Console.WriteLine("4. Play against computer opponent")
        Console.WriteLine("5. Return to main menu")

        Do While valid = False
            valid = True
            Console.WriteLine("What would you like to do? (enter number)")
            choice1 = Console.ReadLine()
            Select Case choice1
                Case "1"
                    choice2 = ""
                    Do Until choice2 = "1" Or choice2 = "2"
                        Console.WriteLine("Press 1 to create a game or 2 to join a game.")
                        choice2 = Console.ReadLine()
                        If choice2 = "1" Then
                            CreateGame()
                        ElseIf choice2 = "2" Then
                            JoinGame()
                        Else
                            Console.WriteLine("Input is invalid.")
                        End If
                    Loop
                Case "2"
                    RandomOnline()
                Case "3"
                    OfflineGame()
                Case "4"
                    PlayAgainstComputer()
                Case "5"
                    Main()
                Case Else
                    Console.WriteLine("Input invalid.")
                    valid = False
            End Select
        Loop
    End Sub

    Sub Leaderboard()

        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("LEADERBOARD:")
        Console.ResetColor()

        Dim searchString As String = "Select * From [Logins] Order By [HighScore] DESC"

        Try
            'assign the database we established at the top of the code to the connection string element of conn
            Dim conn As New OleDbConnection
            conn.ConnectionString = connString
            'Open connection to the database
            conn.Open()

            'Cmd will represent the connection and sql statement that we wish to run on the database 
            Dim cmd As New OleDbCommand(searchString, conn)
            Dim dap As New OleDbDataAdapter
            dap.SelectCommand = cmd

            'using the data adapter, run the query and fill the datatable with the returned data results
            Dim dt As New DataTable
            dap.Fill(dt)

            'close the connection to the database
            conn.Close()

            'looping through each row of results in the datatable we have just populated
            For Each row As DataRow In dt.Rows
                If row("Username") = ListOfPlayers(0).username Then
                    Console.BackgroundColor = ConsoleColor.Cyan
                    Console.ForegroundColor = ConsoleColor.Black
                End If
                Console.WriteLine(row("Username") & " - £" & row("Highscore"))
                Console.ResetColor()
            Next

            dt.Clear()

            'catch any errors in the process and output them if necessary
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Console.WriteLine()
        Console.WriteLine("Press enter to return to the main menu.")
        Console.ReadLine()
        MainMenu()

    End Sub

    Sub Instructions()

        Dim NumberOfLines, i As Integer
        NumberOfLines = 0

        Dim reader As New System.IO.StreamReader("C:\MonopolyInstructions.txt")
        Dim allLines As List(Of String) = New List(Of String)
        Do While Not reader.EndOfStream
            allLines.Add(reader.ReadLine())
            NumberOfLines += 1
        Loop
        reader.Close()

        For i = 0 To NumberOfLines - 1
            Console.WriteLine((allLines(i)))
        Next

        Console.WriteLine("PRESS ENTER TO RETURN TO THE MAIN MENU.")
        Console.ReadLine()
        MainMenu()

    End Sub

    Sub CreateGame()

        'NEED TO GENERATE RANDOM CODE OF LETTERS AND NUMBERS
        Console.WriteLine("Game code: ")
        Console.ReadLine()

    End Sub

    Sub JoinGame()

        Console.WriteLine("Please enter game code.")
        Console.ReadLine()

    End Sub

    Sub RandomOnline()

    End Sub

    Sub OfflineGame()

        Dim input As String

        Do
            Console.WriteLine()
            Console.WriteLine("Current players are: ")
            For i = 0 To ListOfPlayers.Count - 1
                Console.BackgroundColor = ListOfPlayers(i).PlayerColour
                Console.WriteLine(ListOfPlayers(i).username)
                Console.BackgroundColor = ConsoleColor.Black
            Next
            Console.WriteLine("Press 1 to add a player, press 2 to create an account, press 3 to remove a player, and press 4 to start the game.")
            Console.WriteLine("(you must have a minimum of 2 players and a maximum of 6 players)")
            input = Console.ReadLine()
            If input = "1" Then
                If ListOfPlayers.Count = 6 Then
                    Console.WriteLine("You already have 6 players, you must remove a player before adding a new one.")
                    Console.ReadLine()
                Else
                    Dim player As Player = Login(ColourOptions)
                    If Not IsNothing(player) And Not ListOfPlayers.Contains(player) Then
                        ListOfPlayers.Add(player)
                    End If
                End If
            ElseIf input = "2" Then
                CreateAccount()
            ElseIf input = "3" Then
                RemovePlayerFromGame()
            ElseIf input = "4" And ListOfPlayers.Count = 1 Then
                Console.WriteLine("You must have a minimum of 2 players.")
            End If
        Loop Until input = "4" And ListOfPlayers.Count > 1

        Game()

    End Sub

    Sub PlayAgainstComputer()

        Dim NumberOfComputerOpponents As Integer

        Do
            Console.WriteLine()
            Console.WriteLine("How many computer opponents would you like to play against?")
            Console.WriteLine("(enter a number between 1 and 5, or enter x to exit)")
            Dim number As String = Console.ReadLine()
            If number.ToLower = "x" Then
                MainMenu()
                Exit Sub
            End If
            For i = 0 To 5
                If number = i Then
                    NumberOfComputerOpponents = i
                    Exit Do
                End If
            Next
            Console.WriteLine()
            Console.WriteLine("Input invalid.")
        Loop

        For i = 1 To NumberOfComputerOpponents
            Dim PlayerColour As ConsoleColor = ColourOptions.Dequeue()
            ListOfPlayers.Add(New ComputerPlayer("ComputerPlayer" & i, ListOfBoardPositions, PlayerColour))
        Next


        Game()

    End Sub

    Sub TitleDeeds(Square)

        Dim NumberOfLines As Integer

        Select Case Square.GetType()
            Case GetType(BoardStation)
                NumberOfLines = 7
            Case GetType(BoardUtility)
                NumberOfLines = 5
            Case GetType(BoardProperty)
                NumberOfLines = 14
        End Select

        Dim CurrentLine As Integer = 0

        Dim i, line As Integer

        Dim reader As New System.IO.StreamReader("C:\MonopolyTitleDeeds.txt")
        Dim allLines As List(Of String) = New List(Of String)
        Do While Not reader.EndOfStream
            allLines.Add(reader.ReadLine())
            If allLines(CurrentLine) = Square.Name.ToUpper Then
                line = CurrentLine
            End If
            CurrentLine += 1
        Loop
        reader.Close()

        For i = line To (line + NumberOfLines)
            Console.WriteLine((allLines(i)))
        Next

        Console.ReadLine()

    End Sub

    Function HasPlayerLeft(player As Player) As Boolean

        Return player.HasLeft

    End Function

    Sub RemovePlayerFromGame()

        Dim PlayerToRemove As String = ""
        Dim found As Boolean = False

        Console.WriteLine()
        Console.WriteLine("Which player would you like to remove?")
        Console.WriteLine("Current players are :")
        For Each player As Player In ListOfPlayers
            Console.BackgroundColor = player.PlayerColour
            Console.WriteLine(player.username)
            Console.BackgroundColor = ConsoleColor.Black
        Next
        Console.WriteLine()
        Do Until found = True Or PlayerToRemove = "5"
            Console.WriteLine("Enter the player's username to remove them, or enter 5 to exit")
            PlayerToRemove = Console.ReadLine()
            If PlayerToRemove = "5" Then
                Exit Do
            Else
                For Each player As Player In ListOfPlayers
                    If PlayerToRemove = player.username Then
                        found = True
                        ListOfPlayers.Remove(player)
                        Console.BackgroundColor = player.PlayerColour
                        Console.Write(player.username)
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.WriteLine(" has been removed.")
                        Exit Do
                    End If
                Next
            End If
            Console.WriteLine("Input invalid.")
        Loop

    End Sub

End Module
