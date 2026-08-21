using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// 화면 전반에서 반복 사용하는 공통 UI 헬퍼.
    /// VisionIns 솔루션의 ComnFunc/ComnEtcFunc/ComnMethod에 3중으로 중복 구현되어 있던
    /// 메세지박스 생성 로직을 하나로 통합한 것이다(gp_ 접두는 원본과의 대응 관계를 위해 유지).
    /// </summary>
    public static class ComnFunc
    {
        /// <summary>확인형 메세지박스(알림/경고/오류).</summary>
        public static void gp_PrintMessage(string message, string title, MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.알림:
                    XtraMessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case MessageType.경고:
                    XtraMessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case MessageType.오류:
                    XtraMessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        /// <summary>질문형 메세지박스(경고/질문). 예(Yes) 선택 시 true.</summary>
        public static bool gp_PrintQuestion(string message, string title, MessageType messageType)
        {
            if (messageType == MessageType.경고)
            {
                return XtraMessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
            }

            if (messageType == MessageType.질문)
            {
                return XtraMessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            }

            return false;
        }
    }
}
