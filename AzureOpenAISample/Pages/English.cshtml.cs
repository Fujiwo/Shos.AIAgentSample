using AzureOpenAISample.Models;

namespace AzureOpenAISample.Pages
{
	public class EnglishModel : AIChatModel
	{
		protected override string PageName => "English";

		protected override int PromptKind => 1;

		protected override string SystemMessage => @"���Ȃ��͎��̉p��b�̑���Ƃ��āA�l�C�e�B�u�b�҂Ƃ��ĐU�镑���Ă��������B
            ���̔����ɑ΂��āA�ȉ��̃t�H�[�}�b�g��1���1���񓚂��܂��B
            �����͏����Ȃ��ł��������B�܂Ƃ߂ĉ�b���e�������Ȃ��ł��������B

            #�t�H�[�}�b�g:
            �y�C���z:
            {���̉p�������R�ȉp��ɒ����Ă��������Blang:en}
            �y���R�z:
            {���̉p���ƁA�������p���̍����ŁA�d�v�ȃ~�X������ꍇ�̂݁A40�����ȓ��ŁA���{��Ŏw�E���܂��Blang:ja}
            �y�ԓ��z:
            {���Ȃ��̉�b���ł��B1���1�̉�b�̂ݏo�͂��܂��B�܂��́A���̔����ɑ��Ƃ�ł��A���̂��ƁA���ւ̎����Ԃ��Ă��������Blang:en}

            #
            ���̍ŏ��̉�b�́A""Hello""�ł��B
            ����A�t�H�[�}�b�g�����i�Ɏ��A�y�C���z�A�y���R�z�A�y�ԓ��z�A��K���o�͂��Ă��������B";

		public EnglishModel(AzureOpenAIContext context) : base(context)
		{}
	}
}