namespace Novel.Command
{
    /// <summary>
    /// ���̃C���^�[�t�F�[�X�������R�}���h�́A����������Ŕ��΂��ꂽ�ۂ�CallZone()���Ăяo����܂�
    /// </summary>
    public interface IZoneCommand
    {
        void CallZone();
    }

    /* Zone�R�}���h�ɂ���
    ����͎�ɃR�}���h���ɃZ�[�u�A���[�h������ۂɗ��p���邱�Ƃ�z�肵���@�\�ł�
    IZoneCommand�C���^�[�t�F�C�X���p������R�}���h�́A�u���̃R�}���h��ʂ����甭�΂���v�ɉ����A
    �u���̃R�}���h��艺����Execute�����ꍇ�����m����v�Ƃ����U��܂����ǉ�����܂�

    ��̓I�Ȏg�p��Ƃ��āABGM�̕ύX���������܂�
    �]���̃R�}���h���ƁA���[�h�Ȃǂɂ��ABGM���Đ�����R�}���h��艺����Execute���ꂽ�ꍇ�ɁA
    BGM�̍Đ�������Ȃ���Ԃł����AZone�R�}���h���g���΂�������m�ł��A�ȒP�ɐ����������������ł��܂�

    �Ȃ��A�R�}���h�̃N���X���́���ZoneCommand�𐄏����܂�
    Flowchart.isCheckZone��false�ɂ��邱�ƂŁAZone�R�}���h���g�p���Ȃ��ꍇ�͏������J�b�g�ł��܂�
    */
}