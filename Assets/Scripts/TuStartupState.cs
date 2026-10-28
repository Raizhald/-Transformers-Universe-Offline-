using System;

public sealed class TuStartupState
{
    public enum Screen { Launcher, Splash, Login, FactionSelect, HangarLoading, Hangar, Error }
    public Screen Current {get;private set;}
    public string CommanderName {get;private set;}
    public string Faction {get;private set;}
    public string Message {get;private set;}
    public TuStartupState(){Current=Screen.Launcher;CommanderName="Commander";Faction="";Message="";}
    public bool Play(){if(Current!=Screen.Launcher)return false;Current=Screen.Splash;return true;}
    public bool FinishSplash(){if(Current!=Screen.Splash)return false;Current=Screen.Login;return true;}
    public bool Login(string name)
    {
        if(Current!=Screen.Login)return false;name=(name??"").Trim();
        if(name.Length<2||name.Length>24){Message="Enter a commander name with 2â€“24 characters.";return false;}
        foreach(char c in name)if(!((c>='a'&&c<='z')||(c>='A'&&c<='Z')||(c>='0'&&c<='9')||c==' '||c=='_'||c=='-')){Message="Use letters, numbers, spaces, underscores or hyphens.";return false;}
        CommanderName=name;Message="";Current=Screen.FactionSelect;return true;
    }
    public bool SelectFaction(string faction)
    {
        if(Current!=Screen.FactionSelect)return false;
        if(!string.Equals(faction,"Decepticons",StringComparison.Ordinal)&&!string.Equals(faction,"Autobots",StringComparison.Ordinal)){Message="Choose Autobots or Decepticons.";return false;}
        Faction=faction;
        Current=Screen.HangarLoading;Message="";return true;
    }
    public bool FinishLoading(){if(Current!=Screen.HangarLoading)return false;Current=Screen.Hangar;return true;}
    public void Back(){if(Current==Screen.FactionSelect)Current=Screen.Login;else if(Current==Screen.Login)Current=Screen.Launcher;Message="";}
    public void Fail(string message){Message=message;Current=Screen.Error;}
}
