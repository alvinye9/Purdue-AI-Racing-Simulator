using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Autonoma
{

public class CanDBCManager
{
    public static String dbcPath = "CAN1-INDY-V17.dbc";

    private String file_contents;
    public List<CanMessageDef> message_defs = new List<CanMessageDef>();

    public SortedDictionary<String, CanMessageDef> message_def_map = new SortedDictionary<String, CanMessageDef>();

    private static String EntryRegexString = @"(^BO_.*?(^[\r\n]|\z))";
    private static RegexOptions EntryRegexOptions = RegexOptions.Compiled | 
        RegexOptions.Multiline |  RegexOptions.Singleline;
    private static Regex EntryRegex = new Regex(EntryRegexString, EntryRegexOptions);

    static CanDBCManager instance;
    public static CanDBCManager GetInstance()
    {
        if(instance == null)
        {
            instance = new CanDBCManager(Path.Combine(Application.streamingAssetsPath, dbcPath));
            Debug.Log("DBC Loaded " + Convert.ToString(instance.message_def_map.Count) + " items", null);

            int le_count = 0;
            int be_count = 0;

            for(int i = 0; i < instance.message_defs.Count; i++)
            {
                le_count += instance.message_defs[i].get_little_endian_signal_count();
                be_count += instance.message_defs[i].get_big_endian_signal_count();
            }

            //Debug.Log(Convert.ToString(le_count) + " LE signals", null);
            //Debug.Log(Convert.ToString(be_count) + " BE signals", null);

        } 
        return instance;
    }

    public static CanMessageDef GetMsgDef(string msg_name)
    {
        return GetInstance().message_def_map[msg_name];
    }

    public CanDBCManager(String in_path)
    {
        dbcPath = in_path;
        file_contents = File.ReadAllText(dbcPath);
        parse_entries();
    }

    public void parse_entries()
    {
        message_defs = new List<CanMessageDef>();
        MatchCollection entry_matches = EntryRegex.Matches(file_contents);
        foreach(Match em in entry_matches)
        {
            CanMessageDef message_def = new CanMessageDef(em);
            message_defs.Add(message_def);
            //message_def.print();
            message_def_map.Add(message_def.name, message_def);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // need path to dbc file   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
}