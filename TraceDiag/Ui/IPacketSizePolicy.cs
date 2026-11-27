namespace TraceDiag.Ui;

/// <summary>Owns packet-size limits and dynamic-size availability.</summary>
internal interface IPacketSizePolicy
{
    void ApplyDontFragment();
    void Refresh();
}
