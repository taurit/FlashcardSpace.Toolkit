import { ReactNode } from "react";
import "./DesktopWindow.iOs.scss";

interface DesktopWindowProps {
    windowClassName: string;
    mainContent: ReactNode;
    bottomContent: ReactNode;
}

const DesktopWindow: React.FC<DesktopWindowProps> = ({ windowClassName, mainContent, bottomContent }) => {
    return (
        <div className={`window-frame ${windowClassName}`}>
            <div className="title-bar">
                <div className="window-controls">
                    <div className="window-control close"></div>
                    <div className="window-control minimize"></div>
                    <div className="window-control maximize"></div>
                </div>
            </div>
            <div className="window-content">{mainContent}</div>
            <div className="window-bottom-content">{bottomContent}</div>
        </div>
    );
};

export default DesktopWindow;
