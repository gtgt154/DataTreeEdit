using System;
using UnityEngine;
[Serializable]
public class ConfigData {
	[SerializeField]
	private int configDataID;
    [SerializeField]
    private string configDataName;
    [SerializeField]
	private int configDataCondition;
	public int GetconfigDataID(){
		return configDataID;
	}
    public string GetconfigDataName()
    {
        return configDataName;
    }
	public int GetconfigDataCondition(){
		return configDataCondition;
	}
	private ConfigData(){}
	public ConfigData(int configDataID = -1, string configDataName = "", int configDataCondition = -1){
		this.configDataID = configDataID;
        this.configDataName = configDataName;
		this.configDataCondition = configDataCondition;
	}
 }

