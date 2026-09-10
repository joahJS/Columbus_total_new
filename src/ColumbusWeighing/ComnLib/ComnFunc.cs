using System;
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
        /// <summary>
        /// 시스템 설정의 "마감 기준 시간"을 반영한 오늘 날짜. 지금 시각이 그 시간보다 이르면
        /// 아직 전날 영업일이 끝나지 않은 것으로 보고 하루 전 날짜를 돌려준다(예: 마감 06:00,
        /// 지금 새벽 3시면 어제 날짜). 형식이 잘못됐거나 자정(00:00, 기본값)이면 일반적인
        /// 오늘 날짜와 동일하다.
        /// </summary>
        public static DateTime GetBusinessToday(string closingTimeText)
        {
            if (!TimeSpan.TryParse(closingTimeText, out var closingTime))
            {
                return DateTime.Today;
            }

            return DateTime.Now.TimeOfDay < closingTime ? DateTime.Today.AddDays(-1) : DateTime.Today;
        }

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
