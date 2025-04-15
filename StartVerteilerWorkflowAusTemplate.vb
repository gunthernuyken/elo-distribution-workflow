''' <summary>
''' 
''' </summary>
''' <param name="objId"></param>
''' <param name="workflowName"></param>
''' <param name="emailAdressen"></param>
''' <returns></returns>



    Public Function StartVerteilerWorkflowAusTemplate(objId As String, workflowName As String, emailAdressen As List(Of String)) As Integer

        ' 1. Benutzer auflösen
        Dim userIds As List(Of String) = GetUserIdsByEmails(emailAdressen)
        If userIds.Count = 0 Then
            Throw New Exception("Keine gültigen Benutzer für die angegebenen E-Mail-Adressen gefunden.")
        End If

        ' 2. Workflow-Vorlage laden
        Dim wf As WFDiagram = IXConnection.Ix.checkoutWorkFlow("Verteiler Werksauftrag", WFTypeC.TEMPLATE, WFDiagramC.mbAll, LockC.NO)

        ' 3. Vorlage aktivieren
        wf.type = WFTypeC.ACTIVE
        wf.id = -1
        wf.name = workflowName
        wf.objId = objId

        ' 4. Platzhalterknoten suchen
        Dim templateNode As WFNode = wf.nodes.FirstOrDefault(Function(n) n.name = "EMPFAENGER_TEMPLATE")
        If templateNode Is Nothing Then Throw New Exception("Platzhalterknoten 'EMPFAENGER_TEMPLATE' nicht gefunden.")

        ' 5. Endknoten suchen (Ziel)
        Dim endNode As WFNode = wf.nodes.FirstOrDefault(Function(n) n.name = "node 2")
        If endNode Is Nothing Then Throw New Exception("Kein Endknoten gefunden.")

        ' 6. Neue Knoten vorbereiten
        Dim neueKnoten As New List(Of WFNode)
        neueKnoten.AddRange(wf.nodes.Where(Function(n) n.name <> "EMPFAENGER_TEMPLATE"))

        ' 7. Benutzerknoten erzeugen
        Dim x As Integer = templateNode.posX
        Dim y As Integer = templateNode.posY
        Dim idCounter As Integer = wf.nodes.Max(Function(n) n.id) + 1
        Dim neueAssocs As New List(Of WFNodeAssoc)

        For Each uidStr In userIds
            Dim userNode As New WFNode(templateNode)
            userNode.id = idCounter
            userNode.name = "Verteiler: " & uidStr
            userNode.userId = Integer.Parse(uidStr)
            userNode.comment = "Automatisch aus Dokuman hinzugefügt"
            userNode.posX = x
            userNode.posY = y

            neueKnoten.Add(userNode)

            ' Verbindung zum Endknoten
            neueAssocs.Add(New WFNodeAssoc With {.nodeFrom = userNode.id, .nodeTo = endNode.id})

            x += 150
            idCounter += 1
        Next

        ' 8. Knoten und Verbindungen eintragen
        wf.nodes = neueKnoten.ToArray()
        wf.matrix = New WFNodeMatrix With {
        .assocs = neueAssocs.ToArray()
    }

        ' 9. Workflow starten
        Try
            Dim flowId As Integer = IXConnection.Ix.checkinWorkFlow(wf, WFDiagramC.mbAll, LockC.NO)
            Return flowId
        Catch ex As Exception
            Debug.Print("Fehler beim Starten des Workflows: " & ex.ToString)
            Return Nothing
        End Try
    End Function









    ''' <summary>
    ''' Wandelt Mailadressen in ELO-Benutzer-IDs um.
    ''' </summary>
    ''' 
    ''' Holt zu einer Liste von E-Mail-Adressen die entsprechenden User-IDs (sehr performant mit Dictionary-Lookup).

    Private Function GetUserIdsByEmails(emails As List(Of String)) As List(Of String)
        Dim result As New List(Of String)

        ' Alle Usernamen abrufen (IDs)
        Dim usernames As UserName() = IXConnection.Ix.getUserNames(Nothing, CheckoutUsersC.ALL_USERS)
        Dim allIds As String() = usernames.Select(Function(u) u.id.ToString()).ToArray()

        ' Alle Userinfos in einem Rutsch laden
        Dim allUserInfos As UserInfo() = IXConnection.Ix.checkoutUsers(allIds, CheckoutUsersC.BY_IDS, LockC.NO)

        ' Dictionary: E-Mail → User-ID (case-insensitive)
        Dim emailToId As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        For Each ui In allUserInfos
            Dim email As String = TryCast(ui.userProps(1), String)
            If Not String.IsNullOrWhiteSpace(email) AndAlso Not emailToId.ContainsKey(email) Then
                emailToId(email) = ui.id.ToString()
            End If
        Next

        ' Gesuchte E-Mail-Adressen gegen das Dictionary prüfen
        For Each emailToCheck In emails
            If emailToId.ContainsKey(emailToCheck) Then
                result.Add(emailToId(emailToCheck))
            End If
        Next

        Return result
    End Function
