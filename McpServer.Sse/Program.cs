// Model Context Protocol (MCP) �T�[�o�[�̊ȈՂȃT���v��
// - WebApplication ���g���� MCP �T�[�o�[���z�X�g
// - HTTP �g�����X�|�[�g��L���ɂ��邱�ƂŁASSE�iServer-Sent Events�j���g�������A���^�C���ʐM���T�|�[�g
// - ����A�Z���u�����ɒ�`���ꂽ McpServer �c�[����o�^
//
// 1. ���̃v���O���������s����ƃ��[�J���z�X�g�� http://localhost:3001 �ŃT�[�o�[���N��
// 2. �N���C�A���g�� MCP �̃v���g�R���ɏ]���� HTTP �G���h�|�C���g�o�R�Őڑ����ASSE �Ń��A���^�C���ʐM
// 3. �c�[���� `[McpServerToolType]` �N���X���� `[McpServerTool]` �������t�^���ꂽ���\�b�h�Ƃ��Č��J�����
// 4. �e���\�b�h�ɂ� `Description` ������t�^���Ă����ƁA�c�[���̐������N���C�A���g�ɒ񋟂����
//
// �Z�L�����e�B�Ɖ^�p�Ɋւ��钍��:
// - ���J���ł� TLS�A�F�؁A�F�ACORS �ݒ�Ȃǂ���������K�v������

using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// MCP �T�[�o�[���T�[�r�X�ɒǉ����AHTTP �g�����X�|�[�g�iSSE �Ή��j�ƃA�Z���u�����̃c�[����o�^
builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();

var application = builder.Build();

// MCP �̃��[�e�B���O���}�b�v���ăT�[�o�[���N��
application.MapMcp();
application.Run("http://localhost:3001");

// �c�[����`: �e���\�b�h�͌��J�����c�[���Ƃ��ČĂяo���\�ɂȂ�
[McpServerToolType, Description("�V�C��\�񂷂�")] // ���̃N���X���c�[���O���[�v�ł��邱�Ƃ���������
public static class WeatherForecastTool
{
    // �w�肵���ꏊ�̓V�C��\�񂵂܂��B
    // ���� `location` �ɂ͓s���{�����Ȃǂ̕������n���܂��B
    [McpServerTool, Description("�w�肵���ꏊ�̓V�C��\�񂵂܂�")] // �c�[���Ƃ��Č��J���A������t�^
    public static string GetWeatherForecast(
        [Description("�V�C��\�񂵂����s���{����")] // �����ɐ�����t�^
        string location) => location switch {
            "�k�C��" => "����",
            "�����s" => "�܂�",
            "�ΐ쌧" => "�J"  ,
            "���䌧" => "��"  ,
            _       => "�����܂肩�J���Ⴉ���ł�����"
        };
}
