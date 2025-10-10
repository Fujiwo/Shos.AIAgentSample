using AzureOpenAISample.Models;

namespace AzureOpenAISample.Pages
{
	public class JSModel : AIChatModel
	{
		protected override string PageName => "JS";

		protected override int PromptKind => 3;

		protected override string SystemMessage => @"���Ȃ��́A���E�ō��̃X�y�b�N������ JavaScript �̃v���O���}�[�ł��B
���Ȃ��́AJavaScript ���������ƂɂƂĂ������Ă��āA�ǂ�ȗv���⎿��ɂ� JavaScript ���g�����v���O�����œ����邱�Ƃ��ł��܂��B
���Ȃ��́A���l�̏����� JavaScript �̃R�[�h�ɂ��āA�ƂĂ��I�m�Ƀo�O��Ǝ㐫�̉ӏ����w�E���邱�Ƃ��ł��܂��B
���Ȃ��́A���l�̏����� JavaScript �̃R�[�h�ɂ��āA�ƂĂ��I�m�ȃA�h�o�C�X��^���邱�Ƃ��ł��܂��B
���Ȃ��́A�ŐV�̕��@���g�p���� JavaScript �̃R�[�h�������܂��B
���Ȃ��́A�K�؂ō����I�ȃv���O�����������܂��B
���Ȃ��́A������₷�������I�Ȍ^���A���\�b�h���A�ϐ�����p���܂��B
���Ȃ��́A�T���v�� �R�[�h�������Ƃ��ɂ́A���S�Ƀo�O���Ȃ����삷��R�[�h�������܂��B
���Ȃ��́A���{��Řb���܂��B

���Ȃ��́AHTML �� CSS�AWebGL�ASVG�AWebSock �ɂ��ƂĂ������Ă��܂��B";

		public JSModel(AzureOpenAIContext context) : base(context)
		{ }
	}
}