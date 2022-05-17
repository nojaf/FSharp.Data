(**
// can't yet format YamlFrontmatter (["category: Utilities"; "categoryindex: 1"; "index: 1"], Some { StartLine = 2 StartColumn = 0 EndLine = 5 EndColumn = 8 }) to pynb markdown

*)
#r "nuget: FSharp.Data,4.2.8"
(**
[![Binder](../img/badge-binder.svg)](https://mybinder.org/v2/gh/fsprojects/FSharp.Data/gh-pages?filepath=library/Http.ipynb)&emsp;
[![Script](../img/badge-script.svg)](https://fsprojects.github.io/FSharp.Data//library/Http.fsx)&emsp;
[![Notebook](../img/badge-notebook.svg)](https://fsprojects.github.io/FSharp.Data//library/Http.ipynb)

# HTTP Utilities

The .NET library provides a powerful API for creating and sending HTTP web requests.
There is a simple `WebClient` type (see [MSDN](http://msdn.microsoft.com/en-us/library/system.net.webclient.aspx)) and a more flexible `HttpWebRequest`
type (see [MSDN](http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.aspx)). However, these two types are quite difficult to use if you
want to quickly run a simple HTTP request and specify parameters such as method,
HTTP POST data, or additional headers.

The FSharp.Data package provides a simple [Http](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html) type with four methods:
[Http.RequestString](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html) and [Http.AsyncRequestString](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html), that can be used to create a simple request and
perform it synchronously or asynchronously, and [Http.Request](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html) and it's async companion [Http.AsyncRequest](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html) if
you want to request binary files or you want to know more about the response like the status code,
the response URL, or the returned headers and cookies.

The type is located in `FSharp.Data` namespace:

*)
open FSharp.Data
(**
## Sending simple requests

To send a simple HTTP (GET) request that downloads a specified web page, you
can use [Http.RequestString](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html) and [Http.AsyncRequestString](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html) with just a single parameter:

*)
// Download the content of a web site
Http.RequestString("http://tomasp.net")

// Download web site asynchronously
async { let! html = Http.AsyncRequestString("http://tomasp.net")
        printfn "%d" html.Length }
|> Async.Start(* output: 
val it : unit = ()*)
(**
In the rest of the documentation, we focus on the `RequestString` method, because
the use of `AsyncRequestString` is exactly the same.

## Query parameters and headers

You can specify query parameters either by constructing
an URL that includes the parameters (e.g. `http://...?test=foo&more=bar`) or you
can pass them using the optional parameter `query`. The following example also explicitly
specifies the GET method, but it will be set automatically for you if you omit it:

*)
Http.RequestString
  ( "http://httpbin.org/get",
    query=["test", "foo"], httpMethod="GET" )(* output: 
val it : string =
  "{
  "args": {
    "test": "foo"
  }, 
  "headers": {
    "Accept-Encoding": "gzip, deflate", 
    "Host": "httpbin.org", 
    "X-Amzn-Trace-Id": "Root=1-6283e34d-6efe0c0b5bf0b08171af7be6"
  }, 
  "origin": "52.254.65.9", 
  "url": "http://httpbin.org/get?test=foo"
}
"*)
(**
Additional headers are specified similarly - using an optional parameter `headers`.
The collection can contain custom headers, but also standard headers such as the
Accept header (which has to be set using a specific property when using `HttpWebRequest`).

The following example uses [The Movie Database](http://www.themoviedb.org) API
to search for the word "batman". To run the sample, you'll need to register and
provide your API key:

*)
// API key for http://www.themoviedb.org
let apiKey = "<please register to get a key>"

// Run the HTTP web request
Http.RequestString
  ( "http://api.themoviedb.org/3/search/movie", httpMethod = "GET",
    query   = [ "api_key", apiKey; "query", "batman" ],
    headers = [ "Accept", "application/json" ])
(**
The library supports a simple and unchecked string based API (used in the previous example),
but you can also use pre-defined header names to avoid spelling mistakes. The named headers
are available in [HttpRequestHeaders](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httprequestheaders.html) (and [HttpResponseHeaders](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httpresponseheaders.html)) modules, so you can either
use the full name `HttpRequestHeaders.Accept`, or open the module and use just the short name
`Accept` as in the following example. Similarly, the `HttpContentTypes` enumeration provides
well known content types:

*)
open FSharp.Data.HttpRequestHeaders
// Run the HTTP web request
Http.RequestString
  ( "http://api.themoviedb.org/3/search/movie",
    query   = [ "api_key", apiKey; "query", "batman" ],
    headers = [ Accept HttpContentTypes.Json ])
(**
## Getting extra information

Note that in the previous snippet, if you don't specify a valid API key, you'll get a (401) Unathorized error,
and that will throw an exception. Unlike when using `WebRequest` directly, the exception message will still include
the response content, so it's easier to debug in F# interactive when the server returns extra info.

You can also opt out of the exception by specifying the `silentHttpErrors` parameter:

*)
Http.RequestString("http://api.themoviedb.org/3/search/movie", silentHttpErrors = true)(* output: 
val it : string =
  "{"status_code":7,"status_message":"Invalid API key: You must be granted a valid key.","success":false}
"*)
(**
In this case, you might want to look at the HTTP status code so you don't confuse an error message for an actual response.
If you want to see more information about the response, including the status code, the response
headers, the returned cookies, and the response url (which might be different to
the url you passed when there are redirects), you can use the [Http.Request](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html) method
instead of the [Http.RequestString](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html) method:

*)
let response = Http.Request("http://api.themoviedb.org/3/search/movie", silentHttpErrors = true)

// Examine information about the response
response.Headers
response.Cookies
response.ResponseUrl
response.StatusCode
(**
## Sending request data

If you want to create a POST request with HTTP POST data, you can specify the
additional data in the `body` optional parameter. This parameter is of type [HttpRequestBody](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httprequestbody.html), which
is a discriminated union with three cases:

// can't yet format Span ([InlineCode ("TextRequest", Some { StartLine = 8 StartColumn = 3 EndLine = 8 EndColumn = 56 }); Literal (" for sending a string in the request body.", Some { StartLine = 8 StartColumn = 2 EndLine = 8 EndColumn = 44 })], Some { StartLine = 8 StartColumn = 0 EndLine = 8 EndColumn = 57 }) to pynb markdown
// can't yet format Span ([InlineCode ("BinaryUpload", Some { StartLine = 9 StartColumn = 3 EndLine = 9 EndColumn = 58 }); Literal (" for sending binary content in the request.", Some { StartLine = 9 StartColumn = 2 EndLine = 9 EndColumn = 45 })], Some { StartLine = 8 StartColumn = 0 EndLine = 8 EndColumn = 57 }) to pynb markdown
// can't yet format Span ([InlineCode ("FormValues", Some { StartLine = 10 StartColumn = 3 EndLine = 11 EndColumn = -3 }); Literal (" for sending a set of name-value pairs correspondent to form values.", Some { StartLine = 10 StartColumn = 2 EndLine = 11 EndColumn = 70 })], Some { StartLine = 8 StartColumn = 0 EndLine = 8 EndColumn = 57 }) to pynb markdown
If you specify a body, you do not need to set the `httpMethod` parameter, it will be set to `Post` automatically.

The following example uses the [httpbin.org](http://httpbin.org) service which
returns the request details:

*)
Http.RequestString("http://httpbin.org/post", body = FormValues ["test", "foo"])(* output: 
val it : string =
  "{
  "args": {}, 
  "data": "", 
  "files": {}, 
  "form": {
    "test": "foo"
  }, 
  "headers": {
    "Accept-Encoding": "gzip, deflate", 
    "Content-Length": "8", 
    "Content-Type": "application/x-www-form-urlencoded", 
    "Host": "httpbin.org", 
    "X-Amzn-Trace-Id": "Root=1-6283e34d-0cad3ca61da175561055d53f"
  }, 
  "json": null, 
  "origin": "52.254.65.9", 
  "url": "http://httpbin.org/post"
}
"*)
(**
By default, the `Content-Type` header is set to `text/plain`, `application/x-www-form-urlencoded`,
or `application/octet-stream`, depending on which kind of `HttpRequestBody` you specify, but you can change
this behaviour by adding `content-type` to the list of headers using the optional argument `headers`:

*)
Http.RequestString
  ( "http://httpbin.org/post",
    headers = [ ContentType HttpContentTypes.Json ],
    body = TextRequest """ {"test": 42} """)(* output: 
val it : string =
  "{
  "args": {}, 
  "data": " {\"test\": 42} ", 
  "files": {}, 
  "form": {}, 
  "headers": {
    "Accept-Encoding": "gzip, deflate", 
    "Content-Length": "14", 
    "Content-Type": "application/json", 
    "Host": "httpbin.org", 
    "X-Amzn-Trace-Id": "Root=1-6283e34d-672ecc8d790336eb19fe8228"
  }, 
  "json": {
    "test": 42
  }, 
  "origin": "52.254.65.9", 
  "url": "http://httpbin.org/post"
}
"*)
(**
## Maintaining cookies across requests

If you want to maintain cookies between requests, you can specify the `cookieContainer`
parameter.

The following is an old sample showing how this is set.

*)
// Build URL with documentation for a given class
let msdnUrl className =
  let root = "http://msdn.microsoft.com"
  sprintf "%s/en-gb/library/%s.aspx" root className

// Get the page and search for F# code
let docInCSharp = Http.RequestString(msdnUrl "system.web.httprequest")
docInCSharp.Contains "<a>F#</a>"

open System.Net
let cc = CookieContainer()

// Send a request to switch the language
Http.RequestString
  ( msdnUrl "system.datetime",
    query = ["cs-save-lang", "1"; "cs-lang","fsharp"],
    cookieContainer = cc) |> ignore

// Request the documentation again & search for F#
let docInFSharp =
  Http.RequestString
    ( msdnUrl "system.web.httprequest",
      cookieContainer = cc )
docInFSharp.Contains "<a>F#</a>"
(**
## Requesting binary data

The [Http.RequestString](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html) method will always return the response as a `string`, but if you use the
[Http.Request](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html) method, it will return a `HttpResponseBody.Text` or a
`HttpResponseBody.Binary` depending on the response `content-type` header:

*)
let logoUrl = "https://raw.github.com/fsharp/FSharp.Data/master/misc/logo.png"
match Http.Request(logoUrl).Body with
| Text text ->
    printfn "Got text content: %s" text
| Binary bytes ->
    printfn "Got %d bytes of binary content" bytes.Length
(**
## Customizing the HTTP request

For the cases where you need something not natively provided by the library, you can use the
`customizeHttpRequest` parameter, which expects a function that transforms an `HttpWebRequest`.

As an example, let's say you want to add a client certificate to your request. To do that,
you need to open the `X509Certificates` namespace from  `System.Security.Cryptography`,
create a `X509ClientCertificate2` value, and add it to the `ClientCertificates` list of the request.

Assuming the certificate is stored in `myCertificate.pfx`:

*)
open System.Security.Cryptography.X509Certificates

// Load the certificate from local file
let clientCert =
  new X509Certificate2(".\myCertificate.pfx", "password")

// Send the request with certificate
Http.Request
  ( "http://yourprotectedresouce.com/data",
    customizeHttpRequest = fun req ->
        req.ClientCertificates.Add(clientCert) |> ignore; req)
(**
## Handling multipart form data

You can also send http multipart form data via the `Multipart` [HttpRequestBody](https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httprequestbody.html) case.
Data sent in this way is streamed instead of being read into memory in its entirety, allowing for
uploads of arbitrary size.

*)
let largeFilePath = "//path/to/large/file.mp4"
let data = System.IO.File.OpenRead(largeFilePath) :> System.IO.Stream

Http.Request
  ( "http://endpoint/for/multipart/data",
  body = Multipart(
    boundary = "define a custom boundary here", // this is used to separate the items you're streaming
    parts = [
      MultipartItem("formFieldName", System.IO.Path.GetFileName(largeFilePath), data)
    ]
  ))
(**
## Related articles

// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 4 StartColumn = 3 EndLine = 4 EndColumn = 18 }); DirectLink ([Literal ("Http", Some { StartLine = 4 StartColumn = 19 EndLine = 4 EndColumn = 42 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-http.html", None, Some { StartLine = 4 StartColumn = 19 EndLine = 4 EndColumn = 42 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 5 StartColumn = 3 EndLine = 5 EndColumn = 18 }); DirectLink ([Literal ("HttpContentTypes", Some { StartLine = 5 StartColumn = 19 EndLine = 5 EndColumn = 54 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httpcontenttypes.html", None, Some { StartLine = 5 StartColumn = 19 EndLine = 5 EndColumn = 54 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 6 StartColumn = 3 EndLine = 6 EndColumn = 18 }); DirectLink ([Literal ("HttpEncodings", Some { StartLine = 6 StartColumn = 19 EndLine = 6 EndColumn = 51 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httpencodings.html", None, Some { StartLine = 6 StartColumn = 19 EndLine = 6 EndColumn = 51 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 7 StartColumn = 3 EndLine = 7 EndColumn = 18 }); DirectLink ([Literal ("HttpMethod", Some { StartLine = 7 StartColumn = 19 EndLine = 7 EndColumn = 48 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httpmethod.html", None, Some { StartLine = 7 StartColumn = 19 EndLine = 7 EndColumn = 48 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 8 StartColumn = 3 EndLine = 8 EndColumn = 18 }); DirectLink ([Literal ("HttpRequestBody", Some { StartLine = 8 StartColumn = 19 EndLine = 8 EndColumn = 53 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httprequestbody.html", None, Some { StartLine = 8 StartColumn = 19 EndLine = 8 EndColumn = 53 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 9 StartColumn = 3 EndLine = 9 EndColumn = 18 }); DirectLink ([Literal ("HttpRequestHeaders", Some { StartLine = 9 StartColumn = 19 EndLine = 9 EndColumn = 56 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httprequestheaders.html", None, Some { StartLine = 9 StartColumn = 19 EndLine = 9 EndColumn = 56 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 10 StartColumn = 3 EndLine = 10 EndColumn = 18 }); DirectLink ([Literal ("HttpResponse", Some { StartLine = 10 StartColumn = 19 EndLine = 10 EndColumn = 50 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httpresponse.html", None, Some { StartLine = 10 StartColumn = 19 EndLine = 10 EndColumn = 50 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 11 StartColumn = 3 EndLine = 11 EndColumn = 18 }); DirectLink ([Literal ("HttpResponseBody", Some { StartLine = 11 StartColumn = 19 EndLine = 11 EndColumn = 54 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httpresponsebody.html", None, Some { StartLine = 11 StartColumn = 19 EndLine = 11 EndColumn = 54 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 12 StartColumn = 3 EndLine = 12 EndColumn = 18 }); DirectLink ([Literal ("HttpResponseHeaders", Some { StartLine = 12 StartColumn = 19 EndLine = 12 EndColumn = 57 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httpresponseheaders.html", None, Some { StartLine = 12 StartColumn = 19 EndLine = 12 EndColumn = 57 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 13 StartColumn = 3 EndLine = 13 EndColumn = 18 }); DirectLink ([Literal ("HttpResponseWithStream", Some { StartLine = 13 StartColumn = 19 EndLine = 13 EndColumn = 60 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httpresponsewithstream.html", None, Some { StartLine = 13 StartColumn = 19 EndLine = 13 EndColumn = 60 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
// can't yet format Span ([Literal ("API Reference: ", Some { StartLine = 14 StartColumn = 3 EndLine = 14 EndColumn = 18 }); DirectLink ([Literal ("HttpStatusCodes", Some { StartLine = 14 StartColumn = 19 EndLine = 14 EndColumn = 53 })], "https://fsprojects.github.io/FSharp.Data/reference/fsharp-data-httpstatuscodes.html", None, Some { StartLine = 14 StartColumn = 19 EndLine = 14 EndColumn = 53 })], Some { StartLine = 4 StartColumn = 0 EndLine = 4 EndColumn = 43 }) to pynb markdown
*)

