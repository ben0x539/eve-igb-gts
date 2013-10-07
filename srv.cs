using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;

using libGTS.Data.Static;
using libGTS.Data.Dynamic;
using libGTS.Utility;
using libGTS.Routes;
using libGTS.Types;

class Srv {
  static void Main(string[] args) {
    StaticStore.ReadFromFile();
    StaticStore.RebuildHashTables();
    DynamicStore.ReadFromFile();

    var srv = new HttpListener();
    srv.Prefixes.Add("http://*:23455/");
    srv.Start();
    using (srv) {
      for (;;) {
        Handle(srv.GetContext());
      }
    }
  }

  static void Handle(HttpListenerContext c) {
    var req = c.Request;
    var path = req.RawUrl;
    var resp = c.Response;
    using (resp) {
      if (path.StartsWith("/to/")) {
        var headers = req.Headers;
        var end = path.Substring(4);
        var start = headers["EVE_SOLARSYSTEMNAME"];
        if (start == null) {
          resp.StatusCode = (int) HttpStatusCode.BadRequest;
        } else {
          var route = GenerateRoute(start, end);
          if (route == null) {
            resp.StatusCode = (int) HttpStatusCode.BadRequest;
          } else {
            resp.ContentType = "text/plain";
            RespondWith(resp, route);
          }
        }
      } else {
        resp.ContentType = "text/html;charset=utf-8";
        RespondWith(resp, File.ReadAllBytes("index.html"));
      }
    }
  }

  static void RespondWith(HttpListenerResponse resp, string text) {
        var bytes = Encoding.UTF8.GetBytes(text);
        RespondWith(resp, bytes);
  }
  static void RespondWith(HttpListenerResponse resp, byte[] bytes) {
      var outp = resp.OutputStream;
      using (outp) {
        outp.Write(bytes, 0, bytes.Length);
      }
  }

  static string GenerateRoute(string nameStart, string nameEnd) {
    var start = LookupSystem(nameStart);
    var end = LookupSystem(nameEnd);
    if (end == null)
      return null;

    var routeParams = new GateRouteParameters();
    routeParams.RoutingOption = GateRouteOption.UseBridges;
    routeParams.ShipMass = 1f;

    var router = new GateRouter(routeParams);
    List<Vertex> path;
    router.GetShortestPath(start, end, out path);

    var b = new StringBuilder();
    foreach (var v in path) {
      var s = v.SolarSystem;
      b.Append(string.Format("{0}:{1}", s.ID, s.Name));
      if (v.JumpType == JumpType.Bridge) {
        var p = v.Bridge.Pos;
        b.Append(string.Format(" @ {0}-{1}", p.Planet, p.Moon));
      }
      b.Append('\n');
    }
    return b.ToString();
  }

  static SolarSystem LookupSystem(string name) {
    var results = GuiTools.SearchForSystem(name, false);
    if (results.Count == 0)
      return null;
    if (results.Count == 1)
      return results[0];
    var result = results.Find(a => a.Name.ToUpper().Equals(name.ToUpper()));
    if (result != null)
      return result;
    return results[0];
  }
}
