Namespace DataPagers
    Public Class DataPager
        Inherits PlaceHolder

        Public Property SayfaBasinaGosterimSayisi As Integer
        Public Property ToplamKayit As Integer

        Protected Overrides Sub Render(writer As HtmlTextWriter)
            writer.Write(Me.PagerTag())
        End Sub


        Private Shared Function PageIndex() As Integer
            Dim retValue = 1
            Dim url = HttpContext.Current.Request.RawUrl
            If url.Contains("page=") Then
                url = url.Replace(url.Substring(0, url.IndexOf("page=") + 5), String.Empty)
                Try
                    retValue = Integer.Parse(url)
                Catch ex As Exception
                    Debug.WriteLine(ex)
                End Try
            End If

            Return retValue
        End Function

        Private Shared Function PageList(total As Integer, current As Integer) As IEnumerable(Of Integer)
            Dim pages = New List(Of Integer)()
            Dim midStack = New List(Of Integer)()

            If total <= 0 OrElse current > total Then
                Return pages
            End If

            Const MaxPages As Integer = 12

            If MaxPages > total Then
                For i As Integer = 1 To total
                    pages.Add(i)
                Next
            Else
                Const Midle As Integer = (MaxPages - 4) / 2


                pages.Add(1)
                pages.Add(2)

                For i As Integer = current - Midle To (current + Midle)
                    If i > 2 AndAlso i < (total - 1) Then
                        midStack.Add(i)
                    End If
                Next

                If midStack.Count < (MaxPages - 2) Then
                    Dim last = Integer.Parse(midStack(midStack.Count - 1).ToString())
                    For j As Integer = last + 1 To (MaxPages - 2)
                        midStack.Add(j)
                    Next
                End If


                If midStack.Count < (MaxPages - 4) Then
                    midStack.Clear()
                    For k As Integer = total - MaxPages + 3 To (total - 2)
                        midStack.Add(k)
                    Next
                End If

                If Integer.Parse(midStack(0).ToString()) > 3 Then
                    pages.Add(0)
                End If

                pages.AddRange(midStack.[Select](Function(p) Integer.Parse(p.ToString())))

                If Integer.Parse(midStack(midStack.Count - 1).ToString()) < (total - 2) Then
                    pages.Add(0)
                End If

                pages.Add(total - 1)
                pages.Add(total)
            End If

            Return pages
        End Function


        Private Shared Function PageUrl() As String
            Dim path = HttpContext.Current.Request.RawUrl

            If path.Contains("?") Then
                If path.Contains("page=") Then
                    Dim index = path.IndexOf("page=")
                    path = path.Substring(0, index)
                Else
                    If Not path.EndsWith("?") Then
                        path += "&"
                    End If
                End If
            Else
                path += "?"
            End If

            Return HttpUtility.HtmlEncode(path + "page={0}")
        End Function


        Private Function PagerTag() As String

            Dim postsPerPage = SayfaBasinaGosterimSayisi
            If postsPerPage <= 0 Then
                Return ""
            End If

            Dim retValue = String.Empty
            Dim link = String.Format("<a href=""{0}"">{{1}}</a>", PageUrl())
            Const LinkCurrent As String = "<a href=""?"" class=""selected"">{0}</a>"
            Dim linkFirst = String.Format("<a href=""{0}"">{{0}}</a>", PageUrl()) 'lasyaf yosarak
            Const LinkDisabled As String = "<a href=""?"">{0}</a>"

            Dim currentPage = PageIndex()

            Dim postCnt = ToplamKayit

            Dim pagesTotal = If(postCnt Mod postsPerPage = 0, postCnt / postsPerPage, (postCnt / postsPerPage) + 1)
            pagesTotal = Math.Floor(pagesTotal)
            If pagesTotal = 0 Then
                pagesTotal = 1
            End If

            If currentPage > pagesTotal Then
                Return String.Format("<p>{0}</p>", "sonuç yok")
            End If

            If postCnt > 0 AndAlso pagesTotal > 1 Then
                retValue = "<div class=""image-pager"">"

                If currentPage = 1 Then
                    retValue += String.Format(LinkDisabled, "<i class=""fa fa-angle-double-left""></i>")
                Else
                    retValue += String.Format(link, currentPage - 1, "<i class=""fa fa-angle-double-left""></i>")
                End If

                Dim pages = PageList(pagesTotal, currentPage)
                For Each i As Integer In pages.[Select](Function(page) Integer.Parse(page.ToString()))
                    If i = 0 Then
                        retValue += "<a>...</a>"
                    Else
                        If i = currentPage Then
                            retValue += String.Format(LinkCurrent, i)
                        Else
                            If i = 1 Then
                                retValue += String.Format(linkFirst, i)
                            Else
                                retValue += String.Format(link, i, i)
                            End If
                        End If
                    End If
                Next

                If currentPage = pagesTotal Then
                    retValue += String.Format(LinkDisabled, "<i class=""fa fa-angle-double-right""></i>")
                Else
                    retValue += String.Format(link, currentPage + 1, "<i class=""fa fa-angle-double-right""></i>")
                End If

                retValue += "</div>"
            End If

            Return retValue
        End Function
    End Class
End Namespace
