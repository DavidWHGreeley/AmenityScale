/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-02-04  Greeley                 get the local stored user name or create and store it.

export function getBattleCodeFromURL() {
    const params = new URLSearchParams(window.location.search);
    return params.get('code');
}